using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Dark Therion Stats", menuName = "Project H/Monster Stats/Dark Therion Stats")]
    public class DarkTherionStats : MonsterStats
    {
        [Header("Common")]
        [SerializeField, Min(0)] private float _delayBeforeAttack = 1f;
        [Header("Projectile Attack")]
        [SerializeField, Min(0)] private int _projectileAttackPower = 1;
        [SerializeField, Min(1)] private int _projectileCount = 5;
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 5;
        [SerializeField, Min(0.01f)] private float _projectileFireGap = 0.2f;
        [Header("Bullet Attack")]
        [SerializeField, Min(0)] private int _bulletAttackPower = 1;
        [SerializeField, Min(1)] private int _bulletCircleCount = 3;
        [SerializeField, Min(0.01f)] private float _bulletCircleStartRadius = 1f;
        [SerializeField, Min(0.01f)] private float _bulletCircleRadiusStep = 0.5f;
        [SerializeField, Min(0.01f)] private float _bulletCircleSpacing = 0.5f;
        [SerializeField, Min(0.01f)] private float _bulletCircleRadiusSpeedBase = 0.5f;
        [SerializeField, Min(0.01f)] private float _bulletCircleRadiusSpeedStep = 0.5f;
        [SerializeField, Min(0.01f)] private float _bulletAngularSpeed = 90f;
        [Header("Spike Attack")]
        [SerializeField, Min(0)] private int _spikeAttackPower = 1;

        public float DelayBeforeAttack => _delayBeforeAttack;

        public int ProjectileAttackPower => _projectileAttackPower;
        public int ProjectileCount => _projectileCount;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileFireGap => _projectileFireGap;

        public int BulletAttackPower => _bulletAttackPower;
        public int BulletCircleCount => _bulletCircleCount;
        public float BulletCircleStartRadius => _bulletCircleStartRadius;   
        public float BulletCircleRadiusStep => _bulletCircleRadiusStep; 
        public float BulletCircleSpacing => _bulletCircleSpacing;   
        public float BulletCircleRadiusSpeedBase => _bulletCircleRadiusSpeedBase;
        public float BulletCircleRadiusSpeedStep => _bulletCircleRadiusSpeedStep;
        public float BulletAngularSpeed => _bulletAngularSpeed;

        public int SpikeAttackPower => _spikeAttackPower;

        public override int AttackPower => InvalidProperty<int>(nameof(AttackPower));
        public override float AttackCooltime => InvalidProperty<float>(nameof(AttackCooltime));

        private T InvalidProperty<T>(string propertyName)
        {
            throw new InvalidOperationException(
                $"'{propertyName}' property of {nameof(VeliaStats)} is not supported. Use specific properties instead.");
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(DarkTherionStats)), CanEditMultipleObjects]
        protected class DarkTherionStatsEditor : MonsterStatsEditor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var excludings = new List<string>
                {
                    "_attackPower",
                    "_attackCooltime",
                };

                SetSpeedProperty((DarkTherionStats)target, excludings);

                DrawPropertiesExcluding(serializedObject, excludings.ToArray());
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
