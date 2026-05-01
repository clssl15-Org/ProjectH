using Actors.PlayerSystem;
using Infrastructure;
using Rules;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public sealed class StunAttackWeapon : Weapon
    {
        protected override void ApplyDamage(Collider2D collider, IDamageable receiver, Direction knockbackDir)
        {
            if (collider.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeStunDamage(AttackPower);
                return;
            }
        }
    }
}
