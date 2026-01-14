using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class NormalMovement : CharacterState
    {
        [SerializeField]
        private bool useStandaloneAttack = true;

        private float moveSpeed => Player.playerStats.moveSpeed;
        [SerializeField]
        private float acceleration = 50f;
        [SerializeField]
        private float monsterOverlapSpeedMultiplier = 0.7f;

        protected string planarSpeedParameter = "PlanarSpeed";

        public float speedMultiplier => Player.playerStats.moveSpeedMultiplier;

        public override void CheckExitTransition()
        {
            if (!Player.canMove) return;
            if (CharacterActions.jump.Started && CharacterActions.movement.Down)
            {
                CharacterStateController.EnqueueTransition<FallingJump>();
            }

            if (CharacterActions.jump.Started)
            {
                CharacterStateController.EnqueueTransition<Jump>();
            }

            if (CharacterActions.attack.Started)
            {
                if (useStandaloneAttack)
                    Player.DefaultAttack();
            }

            if (CharacterActions.dash.Started)
            {
                CharacterStateController.EnqueueTransition<Dash>();
            }
            if (CharacterActions.changeSkill.Started)
            {
                Player.ChangeSkill(-1);
            }

            if (CharacterActions.useSkill.Started)
            {
                Player.UseSkill();
            }

            if (CharacterActions.rangedAttack.Started)
            {
                Player.RangedAttack();
            }

            if (CharacterActions.ultimate.Started && CharacterActor.IsGrounded)
            {
                Player.UseUltimate();
            }
        }
        public override void UpdateBehaviour(float dt)
        {
            if (!Player.canMove) return;

            ProcessVelocity(dt);

            CharacterActor.ChangeFlipX(CharacterStateController.InputMovementReference);
        }

        private void ProcessVelocity(float dt)
        {
            Vector3 targetVelocity = CharacterStateController.InputMovementReference * moveSpeed * speedMultiplier;
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
