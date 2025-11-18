using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Monster Stats", menuName = "Project H/Monster Stats")]
    public class MonsterStats : ScriptableObject
    {
        [Header("기본 능력치")]
        [SerializeField, Min(1)] private int _maxHP = 10;
        [SerializeField] private bool _useCustomSpeed = false;
        [SerializeField] private MoveSpeed _moveSpeed = Monsters.MoveSpeed.Normal;
        [SerializeField, Min(0)] private float _speed = 0;


        [Header("공격")]
        [SerializeField, Min(0)] private int _attackPower = 1;
        [SerializeField, Min(0)] private float _attackCooltime = 0.5f;
        [SerializeField, Min(0)] private float _invincibleDuration = 0.5f;

        public virtual int MaxHP => _maxHP;
        public virtual float InvincibleDuration => _invincibleDuration;
        public virtual float MoveSpeed => !_useCustomSpeed ? _moveSpeed.ToFloat() : _speed;
        public virtual int AttackPower => _attackPower;
        public virtual float AttackCooltime => _attackCooltime;


#if UNITY_EDITOR
        [CustomEditor(typeof(MonsterStats)), CanEditMultipleObjects]
        protected class MonsterStatsEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                var excludings = new List<string>();
                SetSpeedProperty((MonsterStats)target, excludings);

                DrawPropertiesExcluding(serializedObject, excludings.ToArray());
                serializedObject.ApplyModifiedProperties();
            }

            protected void SetSpeedProperty(MonsterStats target, List<string> excludings)
            {
                if (!target._useCustomSpeed)
                    excludings.Add("_speed");
                else
                    excludings.Add("_moveSpeed");
            }
        }
#endif
    }
}
