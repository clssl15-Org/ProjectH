using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.PlayerSystem
{
    public class Dash : CharacterState
    {
        [Header("Movement Settings")]
        [Min(0f)]
        [SerializeField]
        private float initalVelocity = 10f;

        [Min(0f)]
        [SerializeField]
        private float duration = 0.5f;

        [SerializeField]
        private AnimationCurve movementCurve = AnimationCurve.Linear(1, 1, 0, 0);

        [SerializeField]
        private DirectionMode directionMode = DirectionMode.InputDirection;

        [Header("Invincible Settings")]
        [SerializeField]
        private float invincibleStartTime = 0f;

        [SerializeField]
        private float invincibleEndTime = 1f;

        [Header("Cooldown Settings")]
        [SerializeField]
        private float cooldownDuration = 1f;

        private float dashCursor = 0;

        private Vector2 dashDirection = Vector2.right;

        private bool isDone = true;

        private float currentSpeedMultiplier = 1f;

        private CooldownTiemr cooldownTimer;

        public override bool CheckEnterTransition(CharacterState fromState)
        {
            return !cooldownTimer || !cooldownTimer.IsOnCooldown;
        }

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
                dashDirection = CharacterActor.Forward;
            }

            if (directionMode == DirectionMode.InputDirection)
            {
                Vector2 inputDirection = CharacterStateController.InputMovementReference;

                if (inputDirection != Vector2.zero)
                {
                    dashDirection = inputDirection;
                    CharacterActor.ChangeFlipX(inputDirection);
                }
                else
                {
                    dashDirection = CharacterActor.Forward;
                }
            }

            ResetDash();
            cooldownTimer = gameObject.AddComponent<CooldownTiemr>();
            cooldownTimer.StartCooldown(cooldownDuration, dt);
        }

        public override void UpdateBehaviour(float dt)
        {
            Vector2 dashVelocity = initalVelocity * currentSpeedMultiplier * movementCurve.Evaluate(dashCursor) * dashDirection;

            CharacterActor.Velocity = dashVelocity;

            float animationDt = dt / duration;
            dashCursor += animationDt;

            if (dashCursor >= invincibleStartTime && dashCursor <= invincibleEndTime)
            {
                Player.Invincible = true;
            }
            else
            {
                Player.Invincible = false;
            }

            if (dashCursor >= 1)
            {
                isDone = true;
                dashCursor = 0f;
            }
        }

        public override void ExitBehaviour(float dt)
        {
            Player.Invincible = false;
        }

        public void ResetDash()
        {
            isDone = false;
            dashCursor = 0;
        }
    }
}
