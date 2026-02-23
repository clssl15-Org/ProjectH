using System;
using System.Collections;
using System.Collections.Generic;
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
            get => invincible;
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

        public float CurrentSkillCooldown
        {
            get
            {
                CooldownTimer cooldownTimer = GetComponentInChildren<CooldownTimer>();
                if (cooldownTimer != null)
                {
                    return cooldownTimer.Progress;
                }
                else
                {
                    return -1f;
                }
            }
        }

        [SerializeField] private bool useDebugUltimateGauge = false;
        [SerializeField, Range(0, 1)] private float debugUltimateGauge = 0f;

        public float CurrentUltimateCooldown
        {
            get
            {
                if (useDebugUltimateGauge)
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

        public event Action Destroying;

        private bool invincible = false;
        private PlatformDetector platformDetector;
        private PlayerHealth playerHealth;
        private SkillManager skillManager;
        private CharacterStateController characterStateController;
        private DamageRoulette damageRoulette;

        // 추가: 플레이어의 현재 방향을 보기 위함
        private SpriteRenderer spriteRenderer;
        
        private void Awake()
        {
            playerStats = originStats.CreateRuntimeStats();
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);

            platformDetector = GetComponent<PlatformDetector>();
            skillManager = GetComponent<SkillManager>();

            characterStateController = GetComponentInChildren<CharacterStateController>();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            damageRoulette = GetComponent<DamageRoulette>();
        }

        public void Inject(PlatformManager platformManager)
        {
            GetComponent<PlatformDetector>().SetPlatformManager(platformManager);
        }

        public void Start()
        {
            ConditionChanged += cond => print($"Player: {cond}");
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

        void OnDestroy()
        {
            Destroying?.Invoke();
        }
    }
}
