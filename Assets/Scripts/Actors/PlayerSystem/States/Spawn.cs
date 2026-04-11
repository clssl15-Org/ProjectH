using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class Spawn : CharacterState
    {
        [SerializeField]
        private float spawnDuration = 1.5f;

        private float spawnCursor = 0f;
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
            ResetState();
        }
        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / spawnDuration;
            spawnCursor += animationDt;

            if (spawnCursor >= 1f)
            {
                isDone = true;
            }
        }
        private void ResetState()
        {
            isDone = false;
        }
    }
}