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

    [Header("Invincible Settings")]
    [SerializeField]
    private float invincibleStartTime = 0f;

    [SerializeField]
    private float invincibleEndTime = 1f;

    private float skillCursor = 0;

    private Vector2 moveDirection = Vector2.right;

    private bool isDone = true;

    private float currentSpeedMultiplier = 1f;

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
}
