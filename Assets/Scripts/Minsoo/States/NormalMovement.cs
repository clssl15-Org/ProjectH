using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovement : CharacterState
{
    public float baseSpeed = 10f;
    public float acceleration = 5f;
    public override void UpdateBehaviour(float dt)
    {
        ProcessVelocity(dt);
    }

    private void ProcessRotation(float dt)
    {
        
    }

    private void ProcessVelocity(float dt)
    {
        Vector3 targetVelocity = CharacterStateController.InputMovementReference * baseSpeed;
        CharacterActor.Velocity = Vector2.MoveTowards(CharacterActor.Velocity, targetVelocity, acceleration * dt);
    }
}
