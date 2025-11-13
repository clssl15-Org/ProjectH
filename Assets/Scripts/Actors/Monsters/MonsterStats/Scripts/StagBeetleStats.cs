using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Stag Beetle Stats", menuName = "Project H/Stag Beetle Stats")]
    public class StagBeetleStats : MonsterStats
    {
        [Header("포효 힐링 비율")]
        [SerializeField, Min(0)] private float _roarHealingRate = 0.1f;

        public float RoarHealingRate => _roarHealingRate;


#if UNITY_EDITOR
        [CustomEditor(typeof(StagBeetleStats)), CanEditMultipleObjects]
        protected class StagBeetleStatsEditor : MonsterStatsEditor { }
#endif
    }
}
