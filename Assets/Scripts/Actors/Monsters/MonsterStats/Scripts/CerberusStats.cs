using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Cerberus Stats", menuName = "Project H/Monster Stats/Cerberus Stats")]
    public class CerberusStats : MonsterStats
    {
        [Header("기준 거리")]
        [SerializeField, Min(0)] private float _nearDistance = 1;
        [Header("특수 공격")]
        [SerializeField, Min(0)] private int _biteAttackPower = 1;
        [SerializeField, Min(0)] private int _dropAreaAttackPower = 1;
        [SerializeField, Min(0)] private int _ambushAttackPower = 1;

        public float NearDistance => _nearDistance;
        public int BiteAttackPower => _biteAttackPower;
        public int DropAreaAttackPower => _dropAreaAttackPower;
        public int AmbushAttackPower => _ambushAttackPower;

        public override int AttackPower => InvalidProperty<int>(nameof(AttackPower));

        private T InvalidProperty<T>(string propertyName)
        {
            throw new InvalidOperationException(
                $"'{propertyName}' property of {nameof(VeliaStats)} is not supported. Use specific properties instead.");
        }

        
#if UNITY_EDITOR
        [CustomEditor(typeof(CerberusStats)), CanEditMultipleObjects]
        protected class CerberusStatsEditor : MonsterStatsEditor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var excludings = new List<string>
                {
                    "_attackPower",
                };

                SetSpeedProperty((CerberusStats)target, excludings);

                DrawPropertiesExcluding(serializedObject, excludings.ToArray());
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
