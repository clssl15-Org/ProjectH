using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure;

namespace Actors.PlayerSystem
{
    public class Stun : CharacterState
    {
        [SerializeField]
        private float stunDuration = 0.5f;

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
            ResetSton();
        }

        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / stunDuration;
            hitCursor += animationDt;

            if (hitCursor >= 1f)
            {
                isDone = true;
            }
        }

        private void ResetSton()
        {
            hitCursor = 0f;
            isDone = false;
        }
    }
}
