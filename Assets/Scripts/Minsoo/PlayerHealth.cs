using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystem
{
    public class PlayerHealth : MonoBehaviour
    {
        public Player Player { get; private set; }
        public CharacterStateController CharacterStateController { get; private set; }
        public int MaxHealth
        {
            get => Player.playerStats.maxHealth;
        }
        private int currentHealth;

        private void Awake()
        {
            Player = this.transform.root.GetComponentInChildren<Player>();
            CharacterStateController = this.transform.root.GetComponentInChildren<CharacterStateController>();
            currentHealth = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (Player.Invincible)
                return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
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
            Debug.Log("Player Died");
        }
    }
}
