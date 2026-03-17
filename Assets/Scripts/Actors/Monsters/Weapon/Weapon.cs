using System;
using Infrastructure;
using Rules;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(TriggerContactHandler))]
    public class Weapon : MonoBehaviour, IWeapon
    {
        [field: SerializeField] public int AttackPower { get; set; } = 1;
        [field: SerializeField] public bool DoKnockback { get; set; } = true;
        public float? KnockbackForce { get; set; } = null;

        private Func<Direction?> _tryGetKnockbackDirection;
        private Action _hitPlayer;
        private TriggerContactHandler _contactHandler;

        public void SetKnockbackInfo(Func<Direction?> tryGetKnockbackDirection) =>
            _tryGetKnockbackDirection = tryGetKnockbackDirection;
        public void SetHitPlayerCallback(Action hitPlayer) =>
            _hitPlayer = hitPlayer;

        private void Awake()
        {
            _contactHandler = GetComponent<TriggerContactHandler>();
            _contactHandler.TargetTags = new[] { "Player" };

            _contactHandler.CollisionEntered += c =>
            {
                if (!c.TryGetComponent<IDamageable>(out var receiver))
                    return;

                var knockbackDir = Direction.Center;
                if (DoKnockback)
                {
                    knockbackDir = _tryGetKnockbackDirection?.Invoke()
                        ?? (c.transform.position - transform.position).ToDirection();
                }

                receiver.TakeDamage(
                    AttackPower,
                    knockbackDir,
                    KnockbackForce);

                _hitPlayer?.Invoke();
            };
        }
    }
}
