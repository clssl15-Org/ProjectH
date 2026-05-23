using JetBrains.Annotations;
using UnityEngine;

namespace Actors.PlayerSystem
{
    [CreateAssetMenu(fileName = "New Player Stats", menuName = "Project H/Player Stats")]
    public class PlayerStatsSO : ScriptableObject
    {
        public int maxHealth = 100;
        public int attackPower = 10;
        public float moveSpeed = 5f;
        public int maxDashCount = 1;
        public int maxJumpCount = 2;
        public bool canJumpAfterDash = true;

        public int additionalMaxHealth = 0;
        public int additionalAttackPower = 0;
        public float additionalMoveSpeed = 0f;

        public float maxHeathMultiplier = 1f;
        public float attackPowerMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public float skillPowerMultiplier = 1f;
        public float skillCooldownMultiplier = 1f;
        public float ultimateCooldownMultiplier = 1f;

        public PlayerStats CreateRuntimeStats()
        {
            return new PlayerStats
            {
                maxHealth = this.maxHealth,
                attackPower = this.attackPower,
                moveSpeed = this.moveSpeed,
                maxDashCount = this.maxDashCount,
                maxJumpCount = this.maxJumpCount,
                canJumpAfterDash = this.canJumpAfterDash,

                additionalMaxHealth = this.additionalMaxHealth,
                additionalAttackPower = this.additionalAttackPower,
                additionalMoveSpeed = this.additionalMoveSpeed,

                maxHeathMultiplier = this.maxHeathMultiplier,
                attackPowerMultiplier = this.attackPowerMultiplier,
                moveSpeedMultiplier = this.moveSpeedMultiplier,
                skillPowerMultiplier = this.skillPowerMultiplier,
                skillCooldownMultiplier = this.skillCooldownMultiplier,
                ultimateCooldownMultiplier = this.ultimateCooldownMultiplier
            };
        }
    }
}
