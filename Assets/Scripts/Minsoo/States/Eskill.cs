using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionMode
{
    FacingDirection,
    InputDirection
}

public class Eskill : CharacterState
{
    [Header("Movement Settings")]
    [Min(0f)]
    [SerializeField]
    private float initalVelocity = 10f;

    [Min(0f)]
    [SerializeField]
    private float duration = 0.2f;

    [SerializeField]
    private AnimationCurve movementCurve = AnimationCurve.Linear(1, 1, 0, 0);

    [SerializeField]
    private DirectionMode directionMode = DirectionMode.InputDirection;

    [Header("Attack Range")]
    [SerializeField]
    private Vector2 attackSize = new Vector2(1.0f, 1.0f);

    [SerializeField]
    private Vector2 attackPointOffset = new Vector2(0f, 0f);

    [Header("Invincible Settings")]
    [SerializeField]
    private float invincibleStartTime = 0f;

    [SerializeField]
    private float invincibleEndTime = 1f;

    private float skillCursor = 0;

    private Vector2 moveDirection = Vector2.right;

    private bool isDone = true;

    private float currentSpeedMultiplier = 1f;

    private void TakeDamageToEnemy()
    {
        Vector2 attackPoint = CharacterActor.ColliderCenter + new Vector2(attackPointOffset.x * CharacterActor.Forward.x, attackPointOffset.y);
        float attackAngle = CharacterActor.Rotation.eulerAngles.z;
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            attackPoint,
            attackSize,
            attackAngle
        );

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                return;

            Debug.Log("Enemy hitted! (Eskill)");
            // damageableObject.TakeDamage();
        }
    }
    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }
    public override void EnterBehaviour(float dt)
    {
        if (directionMode == DirectionMode.FacingDirection)
        {
            moveDirection = CharacterActor.Forward;
        }

        if (directionMode == DirectionMode.InputDirection)
        {
            Vector2 inputDirection = CharacterStateController.InputMovementReference;

            if (inputDirection != Vector2.zero)
            {
                moveDirection = inputDirection;
                CharacterActor.ChangeFlipX(inputDirection);
            }
            else
            {
                moveDirection = CharacterActor.Forward;
            }
        }
        
        ResetSkill();

        TakeDamageToEnemy();
    }
    public override void UpdateBehaviour(float dt)
    {
        Vector2 dashVelocity = initalVelocity * currentSpeedMultiplier * movementCurve.Evaluate(skillCursor) * moveDirection;

        CharacterActor.Velocity = dashVelocity;

        float animationDt = dt / duration;
        skillCursor += animationDt;

        if (skillCursor >= invincibleStartTime && skillCursor <= invincibleEndTime)
        {
            // set invincible true
        }
        else
        {
            // set invincible false
        }

        if (skillCursor >= 1)
        {
            isDone = true;
            skillCursor = 0f;
        }
    }
    
    private void ResetSkill()
    {
        isDone = false;
        skillCursor = 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (isDone) return;

        if (CharacterActor == null) return;

        // Set Gizmo color for attack area visualization
        Gizmos.color = Color.green;

        Gizmos.matrix = Matrix4x4.TRS(
            CharacterActor.ColliderCenter + new Vector2(attackPointOffset.x * CharacterActor.Forward.x, attackPointOffset.y),
            CharacterActor.Rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, attackSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
