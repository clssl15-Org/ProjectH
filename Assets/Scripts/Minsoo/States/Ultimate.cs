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

    private float skillCursor = 0f;

    private Vector2 direction = Vector2.right;

    private bool isDone = true;
    private bool isDamageApplied = false;

    private void TakeDamageToEnemy()
    {

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
            TakeDamageToEnemy();
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
}
