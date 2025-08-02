using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : CharacterState
{
    [SerializeField]
    private float jumpForce = 10f;

    private bool isDone = false;
    private float motionTimer = 0f;

    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }
    public override void EnterBehaviour(float dt)
    {
        ResetJump();
        CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, jumpForce);
    }
    public override void UpdateBehaviour(float dt)
    {
        if (CharacterActor.IsGrounded)
        {
            isDone = true;
        }
    }

    public void ResetJump()
    {
        isDone = false;
        motionTimer = 0f;
    }
}
