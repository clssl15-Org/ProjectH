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
            Vector3 targetVelocity = CharacterStateController.InputMovementReference * subsequentJumpSpeed;

            // 점프 상태이거나 공중에 떠 있는 경우 airAcceleration을 사용합니다.
            float currentAcceleration = CharacterActor.IsLanded ? acceleration : airAcceleration;

            // MoveTowards는 수치상 선형 보간을 해주므로, 가속도가 낮을수록 목표 속도에 도달하는 시간이 길어집니다.
            CharacterActor.Velocity = Vector2.MoveTowards(
                CharacterActor.Velocity,
                new Vector2(targetVelocity.x, CharacterActor.Velocity.y), // Y축은 물리 엔진(중력)에 맡기고 X축만 제어
                currentAcceleration * dt
            );
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
