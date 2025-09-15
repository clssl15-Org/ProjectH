using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ultimate : CharacterState
{
    [Header("Skill Timing Settings")]
    [SerializeField]
    private float skillDuration = 1.5f;

    [SerializeField]
    private float damageApplyTime = 0.4f;

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

    [Header("Attack Properties")]
    private Vector2 attackPoint = Vector2.zero;
    private Vector2 scaledSize = new Vector2(1.0f, 1.0f);
    private float attackAngle = 0f;

    [Header("Gizmos")]
    private bool isHitBoxEnabled = false;

    private float skillCursor = 0f;

    private Vector2 direction = Vector2.right;

    private bool isDone = true;
    private bool isDamageApplied = false;

    private void TakeDamageToEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            attackPoint,
            scaledSize,
            attackAngle
        );
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                return;

            Debug.Log("Enemy hitted! (Ultimate)");
            // damageableObject.TakeDamage();
        }
    }
    private void UpdateAttackParameters()
    {
        attackPoint = CharacterActor.ColliderCenter + (new Vector2(attackPointOffset.x * direction.x, attackPointOffset.y)) * CharacterActor.Size;
        scaledSize = attackSize * CharacterActor.Size;
        attackAngle = CharacterActor.Rotation.eulerAngles.z;
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
            direction = CharacterActor.Forward;
        }

        if (directionMode == DirectionMode.InputDirection)
        {
            Vector2 inputDirection = CharacterStateController.InputMovementReference;

            if (inputDirection != Vector2.zero)
            {
                direction = inputDirection;
                CharacterActor.ChangeFlipX(inputDirection);
            }
            else
            {
                direction = CharacterActor.Forward;
            }
        }

        ResetSkill();
        UpdateAttackParameters();
    }

    public override void UpdateBehaviour(float dt)
    {
        float animationDt = dt / skillDuration;
        skillCursor += animationDt;

        if (skillCursor >= invincibleStartTime && skillCursor <= invincibleEndTime)
        {
            // set invincible true
        }
        else
        {
            // set invincible false
        }

        if (skillCursor >= damageApplyTime && !isDamageApplied)
        {
            isDamageApplied = true;
            isHitBoxEnabled = true;
            TakeDamageToEnemy();
        }

        if (skillCursor >= 1)
        {
            isDone = true;
            isHitBoxEnabled = false;
            skillCursor = 0f;
        }
    }

    private void ResetSkill()
    {
        isDone = false;
        isDamageApplied = false;
        isHitBoxEnabled = false;
        skillCursor = 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (isDone) return;

        if (!isHitBoxEnabled) return;

        if (CharacterActor == null) return;

        // Set Gizmo color for attack area visualization
        Gizmos.color = Color.green;

        Gizmos.matrix = Matrix4x4.TRS(
            attackPoint,
            CharacterActor.Rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, scaledSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
