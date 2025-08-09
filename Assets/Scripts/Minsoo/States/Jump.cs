using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : CharacterState
{
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    private float baseSpeed = 5f;
    [SerializeField]
    private float acceleration = 50f;
    [SerializeField]
    private float doubleJumpBufferTime = 0.1f;

    protected string heightParameter = "Height";

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
        ResetJump();
        CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, jumpForce);
    }
    public override void UpdateBehaviour(float dt)
    {

        ProcessVelocity(dt);

        if (CharacterActor.IsLanded)
        {
            isDone = true;
        }
    }
    private void ProcessVelocity(float dt)
    {
        Vector3 targetVelocity = CharacterStateController.InputMovementReference * baseSpeed;
        CharacterActor.Velocity = Vector2.MoveTowards(CharacterActor.Velocity, targetVelocity, acceleration * dt);
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public void ResetJump()
    {
        isDone = false;
    }
}
