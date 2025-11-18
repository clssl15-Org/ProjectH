using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.PlayerSystem
{
    public class Jump : CharacterState
    {
        [SerializeField]
        private float jumpForce = 10f;
        [SerializeField]
        private float subsequentJumpMultiplier = 0.7f;
        [SerializeField]
        private float baseSpeed = 5f;
        [SerializeField]
        private float acceleration = 50f;
        [SerializeField]
        private int maxJumps = 2;
        [SerializeField]
        private float jumpInterval = 0.1f;

        private int extraJumpCount;

        protected string heightParameter = "Height";

        private bool isDone = false;
        private float jumpCursor;
        private float subsequentJumpForce;

        public override void CheckExitTransition()
        {
            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
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
        }
        public override void EnterBehaviour(float dt)
        {
            ResetJump();
            CharacterActor.Rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            //CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, jumpForce);
            extraJumpCount--;
            subsequentJumpForce = jumpForce * subsequentJumpMultiplier;
        }
        public override void UpdateBehaviour(float dt)
        {
            ProcessVelocity(dt);

            float jumpIntervalDt = dt / jumpInterval;
            jumpCursor += jumpIntervalDt;

            if (CharacterActions.jump.Started && extraJumpCount > 0 && (jumpCursor >= 1f))
            {
                CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, subsequentJumpForce);
                extraJumpCount--;
                subsequentJumpForce *= subsequentJumpMultiplier;
                jumpCursor = 0f;

                CharacterActor.Animator.Rebind();
            }

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

        public void ResetJump()
        {
            isDone = false;
            extraJumpCount = maxJumps;
            jumpCursor = 0f;
        }
    }
}
