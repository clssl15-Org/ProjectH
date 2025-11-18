using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure;

namespace Actors.PlayerSystem
{
    [RequireComponent(typeof(PlayerHealth))]
    public class Player : MonoBehaviour, IPlayer
    {
        public int HP => playerHealth.CurrentHealth;

        public PlayerStatsSO playerStats;
        public bool Invincible
        {
            get => invincible;
            set => invincible = value;
        }

        public bool IsAlive => playerHealth.IsAlive;

        public int MaxHP => playerHealth.MaxHealth;

        public int CurrentPlatform { get; private set; } = -1;


        public event Action<PlayerCondition> ConditionChanged;
        public event Action Destroyed;

        private bool invincible = false;
        private PlatformDetector platformDetector;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);

            platformDetector = GetComponent<PlatformDetector>();
        }

        public void Start()
        {
            ConditionChanged += cond => print($"Player: {cond}");
        }

        void Update()
        {
            if (platformDetector.TryGetCurrentPlatformId(out var currentPlatformID))
                CurrentPlatform = currentPlatformID;
            else
                CurrentPlatform = -1;
        }
    }
}
