using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class NoInputState : CharacterState
{
    private bool isDone;
    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }
    public override void EnterBehaviour(float dt)
    {
        ResetState();
    }
    public override void UpdateBehaviour(float dt)
    {
    }
    private void ResetState()
    {
        isDone = false;
    }
}
