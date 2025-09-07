using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : CharacterState
{
    [Header("Skill Timing Settings")]
    [SerializeField]
    private float motionDuration = 1f;

    [SerializeField]
    private float launchDelay = 0.1f;

    [Header("Projectile Settings")]
    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private Vector2 offset;

    private float skillCursor = 0f;

    private bool isDone = true;
    private bool isProjectileLaunched = true;

    public override void CheckExitTransition()
    {
        if (isDone)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }

    public override void EnterBehaviour(float dt)
    {
        ResetSkill();
    }

    public override void UpdateBehaviour(float dt)
    {
        float animationDt = dt / motionDuration;
        skillCursor += animationDt;

        if (skillCursor >= 1f)
        {
            isDone = true;
        }

        if (skillCursor >= launchDelay && !isProjectileLaunched)
        {
            Vector2 position = CharacterActor.ColliderCenter + offset;
            Vector2 direction = CharacterActor.Forward;
            Quaternion rotation = CharacterActor.Rotation;

            GameObject newProjectile = Instantiate(projectilePrefab, position, rotation);
            newProjectile.GetComponent<ProjectileMovement>().ResetProjectile(dt, direction);

            isProjectileLaunched = true;
        }
    }

    private void ResetSkill()
    {
        skillCursor = 0f;
        isDone = false;
        isProjectileLaunched = false;
    }
}
