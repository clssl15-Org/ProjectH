using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlackThunder.BlackboxSystem;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.PlayerSystem
{
    [RequireComponent(typeof(PlayerHealth))]
    public class Player : MonoBehaviour, IPlayer
    {
        public int HP => playerHealth.CurrentHealth;
        public int SelectedSkillIndex
        {
            get => skillManager.SelectedSkillIndex;
        }

        public PlayerStatsSO originStats;
        public PlayerStats playerStats;
        public bool Invincible
        {
            get => invincible || invincibleOverrideSources.Count > 0;
            set => invincible = value;
        }

        public int sieldCount { get; set; } = 0;

        public bool IsAlive => playerHealth.IsAlive;

        public IEnumerable<SkillType> HavingSkills => skillManager.HavingSkills;
        public SkillType SelectedSkillType => skillManager.SelectedSkillType;

        public int MaxHP => playerHealth.MaxHealth;
        public int BaseMaxHP => originStats != null
            ? Mathf.Max(1, (int)((originStats.maxHealth + originStats.additionalMaxHealth) * originStats.maxHeathMultiplier))
            : MaxHP;
        public int AttackPower => (int)((playerStats.attackPower + playerStats.additionalAttackPower) * playerStats.attackPowerMultiplier);

        public bool canMove { get; set; } = true;
        public int CurrentPlatform { get; private set; } = -1;
        public Direction Direction => !spriteRenderer.flipX ? Direction.Right : Direction.Left;

        public int MaxDashCount => playerStats.maxDashCount;
        public int CurrentDashCount { get; set; }
        public int MaxJumpCount => playerStats.maxJumpCount;
        public int CurrentJumpCount { get; set; }

        public bool AllowInput { get; set; } = true;
        bool IInputLayerSubject.IsTrigger { get; } = true;

        public float CurrentSkillCooldown
        {
            get
            {
                var cooldownTimer = GetComponentsInChildren<CooldownTimer>()
                    .Where(c => c.CooldownType == CooldownType.Skill && c.IsOnCooldown)
                    .OrderByDescending(c => c.Progress)
                    .FirstOrDefault();

                return cooldownTimer != null ? cooldownTimer.Progress : -1f;
            }
        }

        [SerializeField] private bool useDebugUltimateGauge = false;
        [SerializeField, Range(0, 1)] private float debugUltimateGauge = 0f;

        public float CurrentUltimateCooldown
        {
            get
            {
                if (useDebugUltimateGauge.Resolve(false))
                    return debugUltimateGauge;

                Ultimate ultimate = GetComponentInChildren<Ultimate>();
                if (ultimate.enabled)
                {
                    return ultimate.CooldownGauge;
                }
                else
                {
                    return -1f;
                }
            }
        }

        public GameObject StatesGO;

        public SkillManager SkillManager => skillManager;

        public float RouletteDamageMultiplier
        {
            get => a;
            set => a = value;
        }

        float a = 1;

        public PlayerHealth PlayerHealth => playerHealth;

        public event Action<PlayerCondition> ConditionChanged;
        public event Action<SkillType> SkillAdded
        {
            add => skillManager.SkillAdded += value;
            remove => skillManager.SkillAdded -= value;
        }
        public event Action<SkillType> SkillChanged
        {
            add => skillManager.SkillChanged += value;
            remove => skillManager.SkillChanged -= value;
        }
        public event Action<float> SkillRouletteApplied;
        public event Action SkillRouletteCleared;

        public event Action Destroying;

        private bool invincible = false;
        private PlatformDetector platformDetector;
        private PlayerHealth playerHealth;
        private SkillManager skillManager;
        private CharacterStateController characterStateController;
        private DamageRoulette damageRoulette;
        private Ultimate ultimate;
        private readonly HashSet<object> invincibleOverrideSources = new();
        private BlackboxHandle _blackbox;

        private static PersistedPlayerState persistedPlayerState;
        private static bool hasPersistedPlayerState;
        private static bool persistEnabled = true;

        private struct PersistedPlayerState
        {
            public PlayerStats Stats;
            public int CurrentHealth;
            public float UltimateGauge;
        }

        // �߰�: �÷��̾��� ���� ������ ���� ����
        private SpriteRenderer spriteRenderer;
        
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("플레이어 초기화를 시작합니다.", out _blackbox);

            playerStats = originStats.CreateRuntimeStats();
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);
            playerHealth.Healed += () => ConditionChanged?.Invoke(PlayerCondition.Heal);
            playerHealth.OnMaxHealthChanged += () => ConditionChanged?.Invoke(PlayerCondition.MaxHealthChanged);
            playerHealth.OnInitialized += () => ConditionChanged?.Invoke(PlayerCondition.Damage);

            platformDetector = GetComponent<PlatformDetector>();
            skillManager = GetComponent<SkillManager>();

            characterStateController = GetComponentInChildren<CharacterStateController>();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            damageRoulette = GetComponent<DamageRoulette>();
            ultimate = GetComponentInChildren<Ultimate>(true);

            if (hasPersistedPlayerState)
            {
                playerStats = persistedPlayerState.Stats;
                playerHealth.SetInitialHealth(persistedPlayerState.CurrentHealth);
            }

            persistEnabled = true;

            if (damageRoulette)
            {
                damageRoulette.BonusApplied += OnSkillRouletteApplied;
                damageRoulette.BonusCleared += OnSkillRouletteCleared;
            }
        }

        public void Inject(PlatformManager platformManager)
        {
            using var _ = _blackbox.Scope("플랫폼 매니저를 주입받습니다.").With(platformManager);

            GetComponent<PlatformDetector>().SetPlatformManager(platformManager);
        }

        public void Start()
        {
            using var _ = _blackbox.Scope("플레이어 시작 설정을 적용합니다.");

            ConditionChanged += cond => print($"Player: {cond}");

            if (hasPersistedPlayerState && ultimate != null)
                ultimate.SetCooldownGauge(persistedPlayerState.UltimateGauge);

            if (LevelManager.Instance != null && LevelManager.Instance.ConsumePlayerDeathRestart())
                characterStateController.EnqueueTransition<Spawn>();
        }

        public static void ClearPersistedProgress()
        {
            using var _ = BlackboxHandle.Of(typeof(Player)).Scope("저장된 플레이어 진행 상태를 초기화합니다.");

            hasPersistedPlayerState = false;
            persistEnabled = false;
        }

        public static void PersistCurrentPlayerProgress()
        {
            using var _ = BlackboxHandle.Of(typeof(Player)).Scope("현재 플레이어 진행 상태 저장을 요청합니다.");

            TryPersistCurrentPlayerProgress();
        }

        public static bool TryPersistCurrentPlayerProgress()
        {
            using var _ = BlackboxHandle.Of(typeof(Player)).Scope("현재 플레이어 진행 상태 저장 가능 여부를 확인합니다.");

            var player = FindObjectOfType<Player>();
            if (player == null || !player.CanPersistProgress())
                return false;

            player.PersistState();
            return true;
        }

        public void DefaultAttack()
        {
            using var _ = _blackbox.Scope("기본 공격을 요청합니다.");

            characterStateController.EnqueueTransition<Attack1>();
        }
        public void RangedAttack()
        {
            using var _ = _blackbox.Scope("원거리 공격을 요청합니다.");

            characterStateController.EnqueueTransition<RangedAttack>();
        }

        void Update()
        {
            if (platformDetector.TryGetCurrentPlatformId(out var currentPlatformID))
                CurrentPlatform = currentPlatformID;
            else
                CurrentPlatform = -1;
        }
        public int CalculateDamage(float value)
        {
            using var _ = _blackbox.Scope($"피해량을 계산합니다. value: {value}");

            int damage = (int)(value * RouletteDamageMultiplier);
            return damage;
        }
        public void ChangeSkill()
        {
            using var _ = _blackbox.Scope("선택 스킬 변경을 요청합니다.");

            skillManager.ChangeSkill();
        }
        public bool TrySkillRoulette(out DamageRoulette.Context context)
        {
            using var _ = _blackbox.Scope("스킬 룰렛 시도를 요청합니다.");

            return damageRoulette.TrySkillRoulette(out context);
        }

        public void ResetRandomSkillBuff()
        {
            using var _ = _blackbox.Scope("랜덤 스킬 버프를 초기화합니다.");

            RouletteDamageMultiplier = 1;
        }

        public void UseSkill()
        {
            using var _ = _blackbox.Scope("스킬 사용을 요청합니다.");

            skillManager.UseSkill();
        }
        public void UseUltimate()
        {
            using var _ = _blackbox.Scope("궁극기 사용을 요청합니다.");

            skillManager.UseUltimate();
        }

        public void NotifyCondition(PlayerCondition condition)
        {
            using var _ = _blackbox.Scope($"플레이어 상태 변경을 알립니다. condition: {condition}");

            ConditionChanged?.Invoke(condition);
        }

        public void SetInvincibleOverride(object source, bool enabled)
        {
            using var _ = _blackbox.Scope($"무적 오버라이드를 설정합니다. enabled: {enabled}").With(source);

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (enabled)
                invincibleOverrideSources.Add(source);
            else
                invincibleOverrideSources.Remove(source);
        }

        private void OnSkillRouletteApplied(float bonus) =>
            SkillRouletteApplied?.Invoke(bonus);

        private void OnSkillRouletteCleared() =>
            SkillRouletteCleared?.Invoke();

        void OnDestroy()
        {
            using var _ = _blackbox.Scope("플레이어를 정리합니다.");

            if (CanPersistProgress())
                PersistState();

            if (damageRoulette)
            {
                damageRoulette.BonusApplied -= OnSkillRouletteApplied;
                damageRoulette.BonusCleared -= OnSkillRouletteCleared;
            }

            Destroying?.Invoke();
        }

        private bool CanPersistProgress()
        {
            return persistEnabled
                && playerHealth != null
                && playerHealth.IsAlive;
        }

        private void PersistState()
        {
            if (playerHealth == null)
                return;

            persistedPlayerState = new PersistedPlayerState
            {
                Stats = playerStats,
                CurrentHealth = playerHealth.CurrentHealth,
                UltimateGauge = ultimate != null ? ultimate.CooldownGauge : 1f
            };
            hasPersistedPlayerState = true;
        }
    }
}
