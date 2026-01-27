using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Werbellion Stats", menuName = "Project H/Monster Stats/Werbellion Stats")]
    public class WerbellionStats : MonsterStats
    {
        [Header("특수 공격")]
        [SerializeField, Min(0)] private int _punchAttackPower = 1;
        [SerializeField, Min(0)] private int _straightAreaAttackPower = 1;
        [SerializeField, Min(0)] private int _spikeAttackPower = 1;
        [SerializeField, Min(0.1f)] private float _spikeSpeed = 5;
        [SerializeField, Min(0.01f)] private float _spikeFireGap = 0.2f;
        [SerializeField, Min(0)] private int _portalAttackPower = 1;
        [SerializeField, Min(0)] private float _stunAttackTime = 1;

        public int PunckAttackPower => _punchAttackPower;
        public int StraightAreaAttackPower => _straightAreaAttackPower;
        public int SpikeAttackPower => _spikeAttackPower;
        public float SpikeSpeed => _spikeSpeed;
        public float SpikeFireGap => _spikeFireGap;
        public int PortalAttackPower => _portalAttackPower;
        public float StunAttackTime => _stunAttackTime;

        public override int AttackPower => InvalidProperty<int>(nameof(AttackPower));

        private T InvalidProperty<T>(string propertyName)
        {
            throw new InvalidOperationException(
                $"'{propertyName}' property of {nameof(WerbellionStats)} is not supported. Use specific properties instead.");
        }

        
#if UNITY_EDITOR
        [CustomEditor(typeof(WerbellionStats)), CanEditMultipleObjects]
        protected class WerbellionStatsEditor : MonsterStatsEditor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var excludings = new List<string>
                {
                    "_attackPower",
                };

                SetSpeedProperty((WerbellionStats)target, excludings);

                DrawPropertiesExcluding(serializedObject, excludings.ToArray());
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
