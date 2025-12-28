using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
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
        private float subsequentJumpSpeedMultiplier = 0.8f;
        [SerializeField]
        private float acceleration = 50f;
        [SerializeField]
        private float airAcceleration = 20f;
        [SerializeField]
        private int maxJumps = 2;
        [SerializeField]
        private float jumpInterval = 0.1f;

        [Header("Juicy Jump Settings")]
        [SerializeField] private float jumpBufferTime = 0.15f; // 점프 입력 저장 시간
        [SerializeField] private float apexBonusMultiplier = 1.2f; // 정점에서 이동 속도 보너스
        [SerializeField] private float apexThreshold = 0.5f;      // 정점으로 판정할 Y축 속도 임계값
        [SerializeField] private float gravityScale = 3f;         // 기본 중력 배율
        [SerializeField] private float fallMultiplier = 1.2f;       // 하강 시 중력 배율
        [SerializeField] private float apexGravityMultiplier = 0.2f;     // 정점에서 중력 배율
        private float jumpBufferCounter; // 버퍼 타이머


        private int extraJumpCount;

        protected string heightParameter = "Height";

        private bool isDone = false;
        private float jumpCursor;
        private float subsequentJumpForce;
        private float subsequentJumpSpeed;

        public override void CheckExitTransition()
        {
            if (isDone)
            {
                // 착지 직전에 점프를 눌러서 버퍼가 남아있다면 다시 Jump 상태를 재시작
                if (jumpBufferCounter > 0f)
                {
                    CharacterStateController.EnqueueTransition<Jump>();
                }
                else
                {
                    CharacterStateController.EnqueueTransition<NormalMovement>();
                }
            }
            if (CharacterActions.attack.Started)
            {
                CharacterStateController.EnqueueTransition<Attack1>();
            }
            if (CharacterActions.dash.Started)
            {
                CharacterStateController.EnqueueTransition<Dash>();
            }
            if (CharacterActions.changeSkill.Started) // TODO:  점프할때도 스킬 변경 가능케
            {
                //CharacterStateController.EnqueueTransition<Eskill>();
            }
        }
        public override void EnterBehaviour(float dt)
        {
            ResetJump();
            ApplyJump(jumpForce);
            //CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, jumpForce);
            //extraJumpCount--;
            //subsequentJumpForce = jumpForce * subsequentJumpMultiplier;
        }
        public override void UpdateBehaviour(float dt)
        {
            ProcessVelocity(dt);

            // 상황에 따른 중력 스케일 조정
            ApplyBetterGravity();

            float jumpIntervalDt = dt / jumpInterval;
            jumpCursor += jumpIntervalDt;

            if (CharacterActions.jump.Started)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= dt;
            }

            if (jumpBufferCounter > 0f && extraJumpCount > 0 && (jumpCursor >= 1f))
            {
                ApplyJump(subsequentJumpForce);
                // 버퍼를 소모했으므로 초기화
                jumpBufferCounter = 0;
            }
            /*
            if (CharacterActions.jump.Started && extraJumpCount > 0 && (jumpCursor >= 1f))
            {
                CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, subsequentJumpForce);
                extraJumpCount--;
                subsequentJumpForce *= subsequentJumpMultiplier;
                jumpCursor = 0f;

                CharacterActor.Animator.Rebind();
            }*/

            if (CharacterActor.IsLanded)
            {
                isDone = true;
            }
        }
        public override void ExitBehaviour(float dt)
        {
            CharacterActor.Rigidbody.gravityScale = gravityScale;
        }
        private void ApplyJump(float force)
        {
            CharacterActor.Velocity = new Vector2(CharacterActor.Velocity.x, force);
            extraJumpCount--;
            jumpCursor = 0f;
            subsequentJumpForce *= subsequentJumpMultiplier;
            subsequentJumpSpeed *= subsequentJumpSpeedMultiplier;
            CharacterActor.Animator.Rebind();
        }
        private void ProcessVelocity(float dt)
        {
            // 정점 판정
            bool isAtApex = Mathf.Abs(CharacterActor.Velocity.y) < apexThreshold;

            // 정점일 때 좌우 이동 속도에 보너스를 주어 포물선을 넓게 만듦
            float currentMaxSpeed = isAtApex ? subsequentJumpSpeed * apexBonusMultiplier : subsequentJumpSpeed;
            float currentAcceleration = CharacterActor.IsLanded ? acceleration : airAcceleration;

            Vector2 targetVelocity = CharacterStateController.InputMovementReference * currentMaxSpeed;

            CharacterActor.Velocity = Vector2.MoveTowards(
                CharacterActor.Velocity,
                new Vector2(targetVelocity.x, CharacterActor.Velocity.y),
                currentAcceleration * dt
            );
        }

        private void ApplyBetterGravity()
        {
            if (CharacterActor.Velocity.y < 0) // 하강 중
            {
                CharacterActor.Rigidbody.gravityScale = gravityScale * fallMultiplier;
            }
            else if (Mathf.Abs(CharacterActor.Velocity.y) < apexThreshold) // 정점 부근
            {
                CharacterActor.Rigidbody.gravityScale = gravityScale * apexGravityMultiplier;
            }
            else // 일반 상승
            {
                CharacterActor.Rigidbody.gravityScale = gravityScale;
            }
        }

        public void ResetJump()
        {
            isDone = false;
            extraJumpCount = maxJumps;
            jumpCursor = 0f;
            subsequentJumpForce = jumpForce;
            subsequentJumpSpeed = baseSpeed;
        }
    }
}
