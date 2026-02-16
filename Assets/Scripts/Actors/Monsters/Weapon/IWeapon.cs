using System;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters
{
    public interface IWeapon
    {
        int AttackPower { get; set; }
        bool DoKnockback { get; set; }
        float? KnockbackForce { get; set; }

        void SetKnockbackInfo(Func<Direction?> tryGetKnockbackDirection);

#pragma warning disable IDE1006
        GameObject gameObject { get; }
        Transform transform { get; }
#pragma warning restore
    }
}
