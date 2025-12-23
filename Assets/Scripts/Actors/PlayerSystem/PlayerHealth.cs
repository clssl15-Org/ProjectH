using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Infrastructure;

namespace Actors.PlayerSystem
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public Player Player { get; private set; }
        public CharacterStateController CharacterStateController { get; private set; }
        public int MaxHealth
        {
            get => Player.playerStats.maxHealth;
        }
        public int CurrentHealth
        {
            get => currentHealth;
        }
        private int currentHealth;
        public bool IsAlive { get; private set; } = true;

        public event Action Damaged;

        private void Awake()
        {
            Player = this.transform.root.GetComponentInChildren<Player>();
            CharacterStateController = this.transform.root.GetComponentInChildren<CharacterStateController>();
            currentHealth = MaxHealth;
        }

        public void TakeDamage(int damage) => TakeDamage(damage, Direction.Center);
        public void TakeDamage(int damage, Direction direction, float? knockbackForce = null)
        {
            if (Player.Invincible)
                return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

            Damaged?.Invoke();
            Debug.Log("Player Health: " + currentHealth + "/" + MaxHealth);
            CharacterStateController.EnqueueTransition<Hit>();
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }

        private void Die()
        {
            IsAlive = false;
            Debug.Log("Player Died");
        }
    }
}
