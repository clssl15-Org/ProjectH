using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Monster Stats", menuName = "Project H/Monster Stats")]
public class MonsterStats : ScriptableObject
{
    [Header("기본 능력치")]
    [SerializeField, Min(1)] private int _maxHP = 10;
    [SerializeField, Min(0)] private int _attackPower = 1;
    [SerializeField, Min(0)] private float _attackCooltime = 0.5f;
    [SerializeField, Min(0)] private float _invincibleDuration = 0.5f;
    [Header("이동")]
    [SerializeField] private bool _useCustomSpeed = false;
    [SerializeField] private MoveSpeed _moveSpeed = global::MoveSpeed.Normal;
    [SerializeField, Min(0)] private float _speed = 0;

    public int MaxHP => _maxHP;
    public int AttackPower => _attackPower;
    public float AttackCooltime => _attackCooltime;
    public float InvincibleDuration => _invincibleDuration;
    public float MoveSpeed => !_useCustomSpeed ? _moveSpeed.ToFloat() : _speed;


#if UNITY_EDITOR
    [CustomEditor(typeof(MonsterStats)), CanEditMultipleObjects]
    protected class MonsterStatsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (MonsterStats)base.target;
            serializedObject.Update();

            if (!target._useCustomSpeed)
                DrawPropertiesExcluding(serializedObject, "_speed");
            else
                DrawPropertiesExcluding(serializedObject, "_moveSpeed");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
