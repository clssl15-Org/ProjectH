using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Ghost Stats", menuName = "Project H/Ghost Stats")]
    public class GhostStats : MonsterStats
    {
        [Header("원거리 공격")]
        [SerializeField, Min(0)] private int _rangedAttackPower = 1;

        public int RangedAttackPower => _rangedAttackPower;


#if UNITY_EDITOR
        [CustomEditor(typeof(GhostStats)), CanEditMultipleObjects]
        protected class GhostStatsEditor : MonsterStatsEditor { }
#endif
    }
}
