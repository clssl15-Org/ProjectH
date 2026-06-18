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
        private int currentHealth = -1;
        private int? initialHealthOverride;
        private int _lastMaxHealth = -1;
        private int _pendingCurrentHealthCap = -1;

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
            if (currentHealth < 0)
            {
                if (_pendingCurrentHealthCap > 0)
                    currentHealth = _pendingCurrentHealthCap;
                else
                    currentHealth = initialHealthOverride.HasValue
                        ? Mathf.Clamp(initialHealthOverride.Value, 0, MaxHealth)
                        : MaxHealth;
            }
            else
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            }

            _lastMaxHealth = MaxHealth;
            OnInitialized?.Invoke();
        }

        public void SetInitialHealth(int health)
        {
            initialHealthOverride = health;
        }
        public void TakeStunDamage(int damage)
        {
            if (ApplyDamage(damage, Direction.Center, null, DamageReaction.Stun))
            {
                print("Player Stunned! Health: " + currentHealth + "/" + MaxHealth);
            }
        }
        public void TakeDamage(int damage) => TakeDamage(damage, Direction.Center);
        public void TakeDamage(int damage, Direction direction, float? knockbackForce = null)
        {
            ApplyDamage(damage, direction, knockbackForce, DamageReaction.Hit);
        }

        private bool ApplyDamage(int damage, Direction direction, float? knockbackForce, DamageReaction damageReaction)
        {
            if (Player.Invincible)
                return false;

            if (Player.sieldCount > 0)
            {
                Player.sieldCount--;
                Debug.Log("Shielded! Remaining Shields: " + Player.sieldCount);

                if (Player.sieldCount == 0)
                {
                    OnSieldBreak?.Invoke();
                }
                return false;
            }

            if (!isInvincivble.Resolve(false))
            {
                currentHealth -= damage;
                currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            }

            RecentKnockback = direction;

            Damaged?.Invoke();
            Debug.Log($"Player Health: {currentHealth} / {MaxHealth} (-{damage}), Knockback: {direction}");
            if (currentHealth <= 0)
            {
                Die();
                return false;
            }
            else
            {
                EnqueueDamageReaction(damageReaction);
                return true;
            }
        }

        private void EnqueueDamageReaction(DamageReaction damageReaction)
        {
            switch (damageReaction)
            {
                case DamageReaction.Stun:
                    CharacterStateController.EnqueueTransition<Stun>();
                    break;
                default:
                    CharacterStateController.EnqueueTransition<Hit>();
                    break;
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
        public void NotifyMaxHealthChanged(int previousMaxHealth)
        {
            int newMax = MaxHealth;

            if (currentHealth < 0 && previousMaxHealth > 0 && newMax > previousMaxHealth)
            {
                _pendingCurrentHealthCap = previousMaxHealth;
            }
            else if (previousMaxHealth > 0 && newMax > previousMaxHealth && currentHealth >= previousMaxHealth)
            {
                currentHealth = previousMaxHealth;
            }
            else if (currentHealth > newMax)
            {
                currentHealth = newMax;
            }

            _lastMaxHealth = newMax;
            OnMaxHealthChanged?.Invoke();
        }

        public void ChangeMaxHealth()
        {
            int previousMax = _lastMaxHealth > 0 ? _lastMaxHealth : MaxHealth;
            NotifyMaxHealthChanged(previousMax);
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

        private enum DamageReaction
        {
            Hit,
            Stun,
        }
    }
}
