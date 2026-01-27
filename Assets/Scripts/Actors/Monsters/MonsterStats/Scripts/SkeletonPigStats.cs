using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Skeleton Pig Stats", menuName = "Project H/Monster Stats/Skeleton Pig Stats")]
    public class SkeletonPigStats : MonsterStats
    {
        [Header("포효 힐링 비율")]
        [SerializeField, Min(0)] private float _roarHealingRate = 0.1f;

        public float RoarHealingRate => _roarHealingRate;


#if UNITY_EDITOR
        [CustomEditor(typeof(SkeletonPigStats)), CanEditMultipleObjects]
        protected class SkeletonPigStatsEditor : MonsterStatsEditor { }
#endif
    }
}
