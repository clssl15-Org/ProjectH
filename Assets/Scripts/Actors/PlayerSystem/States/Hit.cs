using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.PlayerSystem
{
    public class Hit : CharacterState
    {
        [SerializeField]
        private float hitDuration = 0.5f;

        [SerializeField]
        private float knockbackPower = 10f;

        private float hitCursor = 0f;
        private bool isDone = true;
        public override void CheckExitTransition()
        {
            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
            }
        }
        public override void EnterBehaviour(float dt)
        {
            ResetHit();
            TakeKnockback();
        }
        private void TakeKnockback()
        {
            Vector2 knockbackDirection = CharacterActor.Backward;
            CharacterActor.Velocity = Vector2.zero;

            CharacterActor.Rigidbody.AddForce(knockbackDirection * knockbackPower, ForceMode2D.Impulse);
        }
        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / hitDuration;
            hitCursor += animationDt;

            if (hitCursor >= 1f)
            {
                isDone = true;
            }
        }
        private void ResetHit()
        {
            hitCursor = 0f;
            isDone = false;
        }
    }
}
