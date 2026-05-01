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
        public bool isInvincivble = false;

        public Player Player { get; private set; }
        public CharacterStateController CharacterStateController { get; private set; }
        public int MaxHealth
        {
            get => (int)((Player.playerStats.maxHealth + Player.playerStats.additionalMaxHealth) * Player.playerStats.maxHeathMultiplier);
        }
        public int CurrentHealth
        {
            get => currentHealth;
        }
        private int currentHealth;

        /// <summary>
        /// �ӽ� ����
        /// </summary>
        public Direction RecentKnockback { get; set; }
        public bool IsAlive { get; private set; } = true;

        public event Action OnInitialized;
        public event Action Damaged;
        public event Action Healed;
        public event Action OnSieldBreak;
        public event Action OnMaxHealthChanged;

        private void Awake()
        {
            Player = this.transform.root.GetComponentInChildren<Player>();
            CharacterStateController = this.transform.root.GetComponentInChildren<CharacterStateController>();
            
        }

        private void Start()
        {
            currentHealth = MaxHealth;
            OnInitialized?.Invoke();
        }
        public void TakeStunDamage(int damage)
        {
            TakeDamage(damage);
            CharacterStateController.EnqueueTransition<Stun>();
        }
        public void TakeDamage(int damage) => TakeDamage(damage, Direction.Center);
        public void TakeDamage(int damage, Direction direction, float? knockbackForce = null)
        {
            if (Player.Invincible)
                return;

            if (Player.sieldCount > 0)
            {
                Player.sieldCount--;
                Debug.Log("Shielded! Remaining Shields: " + Player.sieldCount);

                if (Player.sieldCount == 0)
                {
                    OnSieldBreak?.Invoke();
                }
                return;
            }

            if (!isInvincivble.Resolve(false))
            {
                currentHealth -= damage;
                currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            }

            RecentKnockback = direction;

            Damaged?.Invoke();
            Debug.Log("Player Health: " + currentHealth + "/" + MaxHealth + $", Knockback: {direction}");
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                CharacterStateController.EnqueueTransition<Hit>();
            }
        }

        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            Healed?.Invoke();
        }
        public void HealByPercent(float percent)
        {
            currentHealth += (int) (MaxHealth * percent);
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            Healed?.Invoke();
        }

        /// <summary>
        /// �ִ�ü���� �������� �� ȣ��
        /// ����� �ִ�ü���� ������Ƽ���� ���ǹǷ� �̺�Ʈ�� �߻�
        /// </summary>
        public void ChangeMaxHealth()
        {
            OnMaxHealthChanged?.Invoke();
        }

        private void Die()
        {
            IsAlive = false;
            Player.AllowInput = false;
            CharacterStateController.EnqueueTransition<Die>();

            Player.NotifyCondition(PlayerCondition.Die);
        }

        public void Stun()
        {
            CharacterStateController.EnqueueTransition<Stun>();
        }
    }
}
