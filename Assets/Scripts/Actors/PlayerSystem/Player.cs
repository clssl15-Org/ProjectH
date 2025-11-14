using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.PlayerSystem
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

        private bool invincible = false;

        private PlayerHealth playerHealth;

        public event Action<PlayerCondition> ConditionChanged;
        public event Action Destroyed;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.Damaged += () => ConditionChanged?.Invoke(PlayerCondition.Damage);
        }

        public void Start()
        {
            ConditionChanged += cond => print($"Player: {cond}");
        }
    }
}
