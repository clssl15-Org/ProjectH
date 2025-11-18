using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class NormalMovement : CharacterState
    {
        [SerializeField]
        private float baseSpeed = 5f;
        [SerializeField]
        private float acceleration = 50f;
        [SerializeField]
        private float monsterOverlapSpeedMultiplier = 0.7f;

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
            float targetSpeedX = targetVelocity.x;

            // Reduce speed when overlapping with monsters
            bool overlappingMonster = false;

            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, transform.localScale * 0.5f, 0f, LayerMask.GetMask("Monster"));
            if (hits.Length > 0)
            {
                overlappingMonster = true;
            }
            targetSpeedX = overlappingMonster ? targetSpeedX * monsterOverlapSpeedMultiplier : targetSpeedX;

            Vector2 currentVelocity = CharacterActor.Velocity;
            float newSpeedX = Mathf.MoveTowards(currentVelocity.x, targetSpeedX, acceleration * dt);

            CharacterActor.Velocity = new Vector2(newSpeedX, currentVelocity.y);
        }

        public override void PostUpdateBehaviour(float dt)
        {
            CharacterStateController.Animator.SetFloat(planarSpeedParameter, CharacterActor.PlanarVelocity.magnitude);
        }
    }
}
