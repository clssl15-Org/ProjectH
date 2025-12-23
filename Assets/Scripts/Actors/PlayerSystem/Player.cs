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

        public PlayerStatsSO playerStats;
        public bool Invincible
        {
            get => invincible;
            set => invincible = value;
        }

        public bool IsAlive => playerHealth.IsAlive;

        public int MaxHP => playerHealth.MaxHealth;

        public int CurrentPlatform { get; private set; } = -1;

        public GameObject StatesGO;

        public SkillManager SkillManager => skillManager;

        public float RouletteDamageMultiplier = 1;


        public event Action<PlayerCondition> ConditionChanged;
        public event Action Destroyed;

        private bool invincible = false;
        private PlatformDetector platformDetector;
        private PlayerHealth playerHealth;
        private SkillManager skillManager;
        private CharacterStateController characterStateController;

        private void Awake()
        {
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
            RouletteDamageMultiplier = 1;
            return damage;
        }
        public void ChangeSkill()
        {
            skillManager.ChangeSkill();
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
            Destroyed?.Invoke();
        }
    }
}
