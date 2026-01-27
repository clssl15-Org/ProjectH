using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Mad Wood Stats", menuName = "Project H/Monster Stats/Mad Wood Stats")]
    public class MadWoodStats : MonsterStats
    {
        [Header("¶¥ °ø°Ý")]
        [SerializeField, Min(0)] private int _groundAttackPower = 1;

        public int GroundAttackPower => _groundAttackPower;


#if UNITY_EDITOR
        [CustomEditor(typeof(MadWoodStats)), CanEditMultipleObjects]
        protected class MadWoodStatsEditor : MonsterStatsEditor { }
#endif
    }
}
