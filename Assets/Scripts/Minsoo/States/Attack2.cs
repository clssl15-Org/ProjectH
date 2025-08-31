using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack2 : CharacterState
{
    [Header("Attack Timing Settings")]
    // The duration of the attack animation
    [SerializeField]
    private float attackDuration = 1f;

    // Time window to allow for combo attacks
    [SerializeField]
    private float comboTimeWindow = 0.3f;

    // Time before the next combo can be initiated
    [SerializeField]
    private float nextComboTime = 0.5f;

    // Duration allowed for chaining the next combo input
    [SerializeField]
    private float comboInputTimeWindow = 0.7f;

    [Header("Attack Stats")]
    [SerializeField]
    private float damageMultiplier = 1.0f;

    [Header("Attack Range")]
    [SerializeField]
    private Vector2 attackSize = new Vector2(1.0f, 1.0f);

    [SerializeField]
    private Vector2 attackPointOffset = new Vector2(0f, 0f);

    [Header("Other Settings")]
    [SerializeField]
    private LayerMask enemyLayers;

    private float attackCursor = 0f;
    private float attackElapsedCursor = 0f;

    private bool comboAvailable = false;
    private bool isDone = true;
    private bool isNextComboReady = false;

    private void Start()
    {
        enemyLayers = LayerMask.GetMask("Monster");
    }

    private void TakeDamageToEnemy()
    {
        Vector2 attackPoint = CharacterActor.ColliderCenter + (attackPointOffset * CharacterActor.Forward);
        float attackAngle = CharacterActor.Rotation.eulerAngles.z;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            attackPoint,
            attackSize,
            attackAngle,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Enemy hitted! (Attack2)");
        }
    }
    public override void CheckExitTransition()
    {
        if (isNextComboReady)
        {
            CharacterStateController.EnqueueTransition<Attack3>();
            return;
        }

        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            CharacterStateController.AddBufferedState<Attack2>();
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

        TakeDamageToEnemy();
    }

    public override void UpdateBehaviour(float dt)
    {
        float animationDt = dt / attackDuration;
        attackCursor += animationDt;

        if (attackCursor >= 1f)
        {
            isDone = true;
        }

        if (!CharacterActions.attack.Started)
            return;

        if (attackCursor >= comboTimeWindow)
        {
            comboAvailable = true;
        }

        if (attackCursor >= nextComboTime && comboAvailable)
        {
            isDone = true;
            isNextComboReady = true;
        }
    }

    public override void UpdateBufferedActions(float dt)
    {
        float animationDt = dt / comboInputTimeWindow;
        attackElapsedCursor += animationDt;

        if (attackElapsedCursor >= 1f)
        {
            CharacterStateController.RemoveBufferedState<Attack2>();
        }

        if (CharacterActions.attack.Started)
        {
            CharacterStateController.EnqueueTransition<Attack3>();
            CharacterStateController.RemoveBufferedState<Attack2>();
        }
    }

    public override void ExitBehaviour(float dt)
    {
        isDone = true;
    }
    private void ResetAttack()
    {
        attackCursor = 0f;
        attackElapsedCursor = 0f;
        comboAvailable = false;
        isDone = false;
        isNextComboReady = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (isDone) return;

        if (CharacterActor == null) return;

        // Set Gizmo color for attack area visualization
        Gizmos.color = Color.green;

        Gizmos.matrix = Matrix4x4.TRS(
            CharacterActor.ColliderCenter + (attackPointOffset * CharacterActor.Forward),
            CharacterActor.Rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, attackSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}