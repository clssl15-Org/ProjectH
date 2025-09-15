using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovement : CharacterState
{
    [SerializeField]
    private float baseSpeed = 5f;
    [SerializeField]
    private float acceleration = 50f;

    protected string planarSpeedParameter = "PlanarSpeed";

    public override void CheckExitTransition()
    {
        if (CharacterActions.jump.Started)
        {
            CharacterStateController.EnqueueTransition<Jump>();
        }

        if (CharacterActions.attack.Started)
        {
            CharacterStateController.EnqueueTransition<Attack1>();
        }

        if (CharacterActions.dash.Started)
        {
            CharacterStateController.EnqueueTransition<Dash>();
        }

        if (CharacterActions.eskill.Started)
        {
            CharacterStateController.EnqueueTransition<Eskill>();
        }

        if (CharacterActions.rangedAttack.Started)
        {
            CharacterStateController.EnqueueTransition<RangedAttack>();
        }

        if (CharacterActions.ultimate.Started)
        {
            CharacterStateController.EnqueueTransition<Ultimate>();
        }
    }
    public override void UpdateBehaviour(float dt)
    {
        ProcessVelocity(dt);

        CharacterActor.ChangeFlipX(CharacterStateController.InputMovementReference);
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
