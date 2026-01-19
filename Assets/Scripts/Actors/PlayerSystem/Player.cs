using System;
using System.Collections;
using System.Collections.Generic;
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

        public int MaxHP => playerHealth.MaxHealth;
        public int AttackPower => (int)((playerStats.attackPower + playerStats.additionalAttackPower) * playerStats.attackPowerMultiplier);

        public bool canMove { get; set; } = true;
        public int CurrentPlatform { get; private set; } = -1;

        public int MaxDashCount => playerStats.maxDashCount;
        public int CurrentDashCount { get; set; }
        public int MaxJumpCount => playerStats.maxJumpCount;
        public int CurrentJumpCount { get; set; }

        public GameObject StatesGO;

        public SkillManager SkillManager => skillManager;

        public List<int> Relics { get; } = new();
        IReadOnlyList<int> IPlayer.Relics => Relics;

        public float RouletteDamageMultiplier
        {
            get => a;
            set => a = value;
        }

        float a = 1;

        public PlayerHealth PlayerHealth => playerHealth;

        public event Action<PlayerCondition> ConditionChanged;
        public event Action Destroyed;
        public event Action<int> RelicAcquired;
        public event Action<int> RelicAbandoned;

        private bool invincible = false;
        private PlatformDetector platformDetector;
        private PlayerHealth playerHealth;
        private SkillManager skillManager;
        private CharacterStateController characterStateController;
        
        private void Awake()
        {
            playerStats = originStats.CreateRuntimeStats();
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);

            platformDetector = GetComponent<PlatformDetector>();
            skillManager = GetComponent<SkillManager>();

            characterStateController = GetComponentInChildren<CharacterStateController>();
        }

        public void Inject(PlatformManager platformManager)
        {
            GetComponent<PlatformDetector>()
                .SetPlatformManager(platformManager);
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
        public void ChangeSkill(int skillIndex)
        {
            skillManager.ChangeSkill();
        }
        public void ApplyRandomSkillBuff(float factor)
        {
            RouletteDamageMultiplier = 1 + factor / 100f;
            //print($"스킬 랜덤 배수 적용: {factor}");
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

        public void OnRelicAcquired(int id) => RelicAcquired?.Invoke(id);

        void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}
