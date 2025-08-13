using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack3 : CharacterState
{
    // The duration of the attack animation
    [SerializeField]
    private float attackDuration = 1f;

    [SerializeField]
    private float damageMultiplier = 1.0f;

    private float attackCursor = 0f;

    private bool comboAvailable = false;
    private bool isDone = false;

    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
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
    }

    private void ResetAttack()
    {
        attackCursor = 0f;
        comboAvailable = false;
        isDone = false;
    }
}
