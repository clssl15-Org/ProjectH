using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    [System.Serializable]
    public struct PlayerStats
    {
        public int maxHealth;
        public int attackPower;
        public float moveSpeed;
        public int maxDashCount;
        public int maxJumpCount;
        public bool canJumpAfterDash;

        public int additionalMaxHealth;
        public int additionalAttackPower;
        public float additionalMoveSpeed;

        public float maxHeathMultiplier;
        public float attackPowerMultiplier;
        public float moveSpeedMultiplier;
        public float skillPowerMultiplier;
        public float skillCooldownMultiplier;
        public float ultimateCooldownMultiplier;
    }
}