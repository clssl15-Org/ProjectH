using UnityEngine;

namespace Actors.PlayerSystem
{
    [CreateAssetMenu(fileName = "New Player Stats", menuName = "Project H/Player Stats")]
    public class PlayerStatsSO : ScriptableObject
    {
        public int maxHealth = 100;
        public int attackPower = 10;
        public float moveSpeed = 5f;

        public int additionalMaxHealth = 0;
        public int additionalAttackPower = 0;
        public float additionalMoveSpeed = 0f;

        public float maxHeathMultiplier = 1f;
        public float attackPowerMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
    }
}
