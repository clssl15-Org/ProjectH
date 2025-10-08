using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Monster Stats", menuName = "Project H/Monster Stats")]
public class MonsterStats : ScriptableObject
{
    // TODO: 몬스터별 능력치 상속으로 구현

    [Header("기본 능력치")]
    [SerializeField, Min(1)] private int maxHP = 10;
    [SerializeField, Min(0)] private int attackPower = 1;
    [SerializeField, Min(0)] private float attackCooltime = 0.5f;
    [SerializeField, Min(0)] private float invincibleDuration = 0.5f;
    [Header("이동")]
    [SerializeField] private bool useCustomSpeed = false;
    [SerializeField] private MoveSpeed moveSpeed = global::MoveSpeed.Normal;
    [SerializeField, Min(0)] private float speed = 0;



    public int MaxHP => maxHP;
    public int AttackPower => attackPower;
    public float AttackCooltime => attackCooltime;
    public float InvincibleDuration => invincibleDuration;
    public float MoveSpeed => !useCustomSpeed ? moveSpeed.ToFloat() : speed;


#if UNITY_EDITOR
    [CustomEditor(typeof(MonsterStats)), CanEditMultipleObjects]
    protected class MonsterStatsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (MonsterStats)base.target;
            serializedObject.Update();

            if (!target.useCustomSpeed)
                DrawPropertiesExcluding(serializedObject, "speed");
            else
                DrawPropertiesExcluding(serializedObject, "moveSpeed");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
