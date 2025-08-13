using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack1 : CharacterState
{
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

    [SerializeField]
    private float damageMultiplier = 1.0f;

    private float attackCursor = 0f;
    private float attackElapsedCursor = 0f;

    private bool comboAvailable = false;
    private bool isDone = false;
    private bool isNextComboReady = false;

    public override void CheckExitTransition()
    {
        if (isNextComboReady)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            return;
        }

        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            CharacterStateController.AddBufferedState<Attack1>();
        }
    }
    public override void EnterBehaviour(float dt)
    {
        CharacterActor.Velocity = new Vector2(0, 0);

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

        if (!CharacterActions.attack.Started)
            return;

        if (attackCursor >= comboTimeWindow)
        {
            comboAvailable = true;
        }

        if (attackCursor >= nextComboTime && comboAvailable)
        {
            isNextComboReady = true;
        }
    }

    public override void UpdateBufferedActions(float dt)
    {
        float animationDt = dt / comboInputTimeWindow;
        attackElapsedCursor += animationDt;

        if (attackElapsedCursor >= 1f)
        {
            CharacterStateController.RemoveBufferedState<Attack1>();
        }

        if (CharacterActions.attack.Started)
        {
            CharacterStateController.EnqueueTransition<Attack2>();
            CharacterStateController.RemoveBufferedState<Attack1>();
        }
    }

    private void ResetAttack()
    {
        attackCursor = 0f;
        attackElapsedCursor = 0f;
        comboAvailable = false;
        isDone = false;
        isNextComboReady = false;
    }
}
