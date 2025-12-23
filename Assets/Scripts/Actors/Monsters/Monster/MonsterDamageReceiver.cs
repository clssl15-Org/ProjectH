using System;
using Infrastructure;
using UnityEngine;
using Rules;

namespace Actors.Monsters
{
    public class MonsterDamageReceiver : MonoBehaviour, IDamageable
    {
        [field: SerializeField] public bool Interactable { get; set; } = true;
        public event Action<DamageInfo> Damaged;

        public void TakeDamage(int damage) =>
            Damaged?.Invoke(new(damage));

        public void TakeDamage(int damage, Direction direction, float? knockbackForce = null) =>
            Damaged?.Invoke(new(damage, direction, knockbackForce));
    }
}
