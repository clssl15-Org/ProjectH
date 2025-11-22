using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Belia Stats", menuName = "Project H/Belia Stats")]
    public class BeliaStats : MonsterStats
    {
        [Header("특수 공격")]
        [SerializeField, Min(0)] private int _slashAttackPower = 1;
        [SerializeField, Min(0)] private int _curvedAreaAttackPower = 1;
        [SerializeField, Min(0)] private int _dashAttackPower = 1;

        public int GroundAttackPower => _slashAttackPower;
        public int CurvedAreaAttackPower => _curvedAreaAttackPower;
        public int DashAttackPower => _dashAttackPower;

        public override int AttackPower => InvalidProperty<int>(nameof(AttackPower));
        public override float AttackCooltime => InvalidProperty<float>(nameof(AttackCooltime));

        private T InvalidProperty<T>(string propertyName)
        {
            throw new InvalidOperationException(
                $"'{propertyName}' property of {nameof(BeliaStats)} is not supported. Use specific properties instead.");
        }

        
#if UNITY_EDITOR
        [CustomEditor(typeof(BeliaStats)), CanEditMultipleObjects]
        protected class BeliaStatsEditor : MonsterStatsEditor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var excludings = new List<string>
                {
                    "_attackPower",
                    "_attackCooltime",
                };

                SetSpeedProperty((BeliaStats)target, excludings);

                DrawPropertiesExcluding(serializedObject, excludings.ToArray());
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
