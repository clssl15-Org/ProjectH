using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovement : CharacterState
{
    protected string planarSpeedParameter = "PlanarSpeed";

    public float baseSpeed = 100f;
    public float acceleration = 1000f;

    public override void CheckExitTransition()
    {
        if (CharacterActions.Jump.Started)
        {
            CharacterStateController.EnqueueTransition<Jump>();
        }
    }
    public override void UpdateBehaviour(float dt)
    {
        ProcessVelocity(dt);
    }

    private void ProcessVelocity(float dt)
    {
        Vector3 targetVelocity = CharacterStateController.InputMovementReference * baseSpeed;
        //Debug.Log("1 : " + CharacterActor.Velocity + ", " + targetVelocity);
        CharacterActor.Velocity = Vector2.MoveTowards(CharacterActor.Velocity, targetVelocity, acceleration * dt);
        //Debug.Log("2 : " + CharacterActor.Velocity);
    }

    public override void PostUpdateBehaviour(float dt)
    {
        CharacterStateController.Animator.SetFloat(planarSpeedParameter, CharacterActor.PlanarVelocity.magnitude);
    }
}
