using System;
using Infrastructure;

namespace Actors
{
    public readonly struct DamageInfo
    {
        public int Damage { get; init; }

        public bool HasKnockback { get; init; }
        public Direction Direction { get; init; }
        public float? KnockbackForce { get; init; }


        public DamageInfo(int damage)
        {
            ThrowIfInvalidDamage(damage);

            Damage = damage;
            HasKnockback = false;

            Direction = default;
            KnockbackForce = default;
        }
        public DamageInfo(int damage, Direction direction, float? knockbackForce = null)
        {
            ThrowIfInvalidDamage(damage);
            ThrowIfInvalidKnockback(knockbackForce);

            Damage = damage;
            HasKnockback = true;
            Direction = direction;
            KnockbackForce = knockbackForce;
        }


        private static void ThrowIfInvalidDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(damage), $"{nameof(damage)} 값은 0 이상이어야 하지만 '{damage}'이(가) 입력되었습니다.");
        }

        private static void ThrowIfInvalidKnockback(float? knockbackForce)
        {
            if (knockbackForce.HasValue && knockbackForce.Value < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(knockbackForce), $"{nameof(knockbackForce)} 값은 null(기본값)이거나 0 이상이어야 하지만 '{knockbackForce.Value}'이(가) 입력되었습니다.");
        }
    }
}
