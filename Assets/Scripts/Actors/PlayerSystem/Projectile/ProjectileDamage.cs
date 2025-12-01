using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;

namespace Actor.PlayerSystem
{
    public class ProjectileDamage : MonoBehaviour
    {
        public int Damage
        {
            get => damage;
            set => damage = value;
        }
        private int damage;

        public static Action onRangedAttack;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                return;

            if(collision.gameObject.CompareTag("Player"))
                return;

            damageableObject.TakeDamage(damage);
            onRangedAttack?.Invoke();
        }
    }
}
