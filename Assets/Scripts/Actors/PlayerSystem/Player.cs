using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            playerStats = originStats.CreateRuntimeStats();
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);
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
            GetComponent<PlatformDetector>().SetPlatformManager(platformManager);
        }

        public void Start()
        {
            ConditionChanged += cond => print($"Player: {cond}");

            if (hasPersistedPlayerState && ultimate != null)
                ultimate.SetCooldownGauge(persistedPlayerState.UltimateGauge);

            if (LevelManager.Instance != null && LevelManager.Instance.ConsumePlayerDeathRestart())
                characterStateController.EnqueueTransition<Spawn>();
        }

        public static void ClearPersistedProgress()
        {
            hasPersistedPlayerState = false;
            persistEnabled = false;
        }

        public static void PersistCurrentPlayerProgress()
        {
            var player = FindObjectOfType<Player>();
            if (player != null)
                player.PersistState();
        }

        public void DefaultAttack()
        {
            characterStateController.EnqueueTransition<Attack1>();
        }
        public void RangedAttack()
        {
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
            int damage = (int)(value * RouletteDamageMultiplier);
            return damage;
        }
        public void ChangeSkill()
        {
            skillManager.ChangeSkill();
        }
        public bool TrySkillRoulette(out DamageRoulette.Context context)
        {
            return damageRoulette.TrySkillRoulette(out context);
        }

        public void ResetRandomSkillBuff()
        {
            RouletteDamageMultiplier = 1;
        }

        public void UseSkill()
        {
            skillManager.UseSkill();
        }
        public void UseUltimate()
        {
            skillManager.UseUltimate();
        }

        public void NotifyCondition(PlayerCondition condition) =>
            ConditionChanged?.Invoke(condition);

        public void SetInvincibleOverride(object source, bool enabled)
        {
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
            if (persistEnabled)
                PersistState();

            if (damageRoulette)
            {
                damageRoulette.BonusApplied -= OnSkillRouletteApplied;
                damageRoulette.BonusCleared -= OnSkillRouletteCleared;
            }

            Destroying?.Invoke();
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
