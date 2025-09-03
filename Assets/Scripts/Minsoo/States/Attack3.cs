using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack3 : CharacterState
{
    [Header("Attack Timing Settings")]
    // The duration of the attack animation
    [SerializeField]
    private float attackDuration = 1f;

    [SerializeField]
    private float damageApplyTime = 0.3f;

    [Header("Attack Stats")]
    [SerializeField]
    private float damageMultiplier = 1.0f;

    [Header("Attack Range")]
    [SerializeField]
    private Vector2 attackSize = new Vector2(1.0f, 0.5f);

    [SerializeField]
    private Vector2 attackPointOffset = new Vector2(0f, 0f);

    [Header("Other Settings")]
    [SerializeField]
    private LayerMask enemyLayers;

    private float attackCursor = 0f;

    private bool comboAvailable = false;
    private bool isDone = true;
    private bool isDamageApplied = false;

    private void Start()
    {
        enemyLayers = LayerMask.GetMask("Monster");
    }

    private void TakeDamageToEnemy()
    {
        Vector2 attackPoint = CharacterActor.ColliderCenter + ( attackPointOffset * CharacterActor.Forward );
        float attackAngle = CharacterActor.Rotation.eulerAngles.z;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            attackPoint,
            attackSize,
            attackAngle,
            enemyLayers
        );
        
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Enemy hitted! (Attack3)");
        }
    }

    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }

        if (CharacterActions.dash.Started)
        {
            CharacterStateController.EnqueueTransition<Dash>();
        }

        if (CharacterActions.eskill.Started)
        {
            CharacterStateController.EnqueueTransition<Eskill>();
        }
    }
    public override void EnterBehaviour(float dt)
    {
        //CharacterActor.Velocity = new Vector2(0, 0);

        ResetAttack();
    }

    public override void UpdateBehaviour(float dt)
    {
        float animationDt = dt / attackDuration;
        attackCursor += animationDt;

        if (attackCursor >= 1f)
        {
            isDone = true;
        }

        if (attackCursor >= damageApplyTime && !isDamageApplied)
        {
            isDamageApplied = true;
            TakeDamageToEnemy();
        }
    }

    public override void ExitBehaviour(float dt)
    {
        isDone = true;
    }

    private void ResetAttack()
    {
        attackCursor = 0f;
        comboAvailable = false;
        isDone = false;
        isDamageApplied = false;
    }

    private void OnDrawGizmos()
    {
        if (isDone) return;

        if (CharacterActor == null) return;

        Gizmos.color = Color.green;

        Gizmos.matrix = Matrix4x4.TRS(
            CharacterActor.ColliderCenter + (attackPointOffset * CharacterActor.Forward),
            CharacterActor.Rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, attackSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
}
