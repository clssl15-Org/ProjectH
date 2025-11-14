using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;

namespace Actor.PlayerSystem
{
    public enum DirectionMode
    {
        FacingDirection,
        InputDirection
    }

    public class Eskill : CharacterState
    {
        [Header("Movement Settings")]
        [Min(0f)]
        [SerializeField]
        private float initalVelocity = 10f;

        [Min(0f)]
        [SerializeField]
        private float duration = 0.2f;

        [SerializeField]
        private AnimationCurve movementCurve = AnimationCurve.Linear(1, 1, 0, 0);

        [SerializeField]
        private DirectionMode directionMode = DirectionMode.InputDirection;

        [Header("Attack Range")]
        [SerializeField]
        private Vector2 attackSize = new Vector2(1.0f, 1.0f);

        [SerializeField]
        private Vector2 attackPointOffset = new Vector2(0f, 0f);

        [Header("Invincible Settings")]
        [SerializeField]
        private float invincibleStartTime = 0f;

        [SerializeField]
        private float invincibleEndTime = 1f;

        [Header("Cooldown Settings")]
        [SerializeField]
        private float cooldownDuration = 7f;

        [Header("Attack Properties")]
        private Vector2 attackPoint = Vector2.zero;
        private Vector2 scaledSize = new Vector2(1.0f, 1.0f);
        private float attackAngle = 0f;

        private float skillCursor = 0;

        private Vector2 direction = Vector2.right;

        private bool isDone = true;

        private float currentSpeedMultiplier = 1f;

        //private HashSet<IDamageable> hitEnemies = new HashSet<IDamageable>();
        private CooldownTiemr cooldownTimer;

        public static Action onEskill;

        private void TakeDamageToEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                attackPoint,
                scaledSize,
                attackAngle
            );

            foreach (Collider2D hitCollider in hitColliders)
            {
                if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                    continue;

                //if (hitEnemies.Contains(damageableObject))
                //    continue;

                //hitEnemies.Add(damageableObject);
                damageableObject.TakeDamage(1);
                onEskill?.Invoke();
            }
        }
        private void UpdateAttackParameters()
        {
            attackPoint = CharacterActor.ColliderCenter + (new Vector2(attackPointOffset.x * direction.x, attackPointOffset.y)) * CharacterActor.Size;
            scaledSize = attackSize * CharacterActor.Size;
            attackAngle = CharacterActor.Rotation.eulerAngles.z;
        }
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
                direction = CharacterActor.Forward;
            }

            if (directionMode == DirectionMode.InputDirection)
            {
                Vector2 inputDirection = CharacterStateController.InputMovementReference;

                if (inputDirection != Vector2.zero)
                {
                    direction = inputDirection;
                    CharacterActor.ChangeFlipX(inputDirection);
                }
                else
                {
                    direction = CharacterActor.Forward;
                }
            }

            ResetSkill();
            cooldownTimer = gameObject.AddComponent<CooldownTiemr>();
            cooldownTimer.StartCooldown(cooldownDuration, dt);
        }
        public override void UpdateBehaviour(float dt)
        {
            Vector2 dashVelocity = initalVelocity * currentSpeedMultiplier * movementCurve.Evaluate(skillCursor) * direction;

            CharacterActor.Velocity = dashVelocity;

            UpdateAttackParameters();
            TakeDamageToEnemy();

            float animationDt = dt / duration;
            skillCursor += animationDt;

            if (skillCursor >= invincibleStartTime && skillCursor <= invincibleEndTime)
            {
                Player.Invincible = true;
            }
            else
            {
                Player.Invincible = false;
            }

            if (skillCursor >= 1)
            {
                isDone = true;
                skillCursor = 0f;
            }
        }

        public override void ExitBehaviour(float dt)
        {
            Player.Invincible = false;
        }

        private void ResetSkill()
        {
            isDone = false;
            skillCursor = 0;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (isDone) return;

            if (CharacterActor == null) return;

            // Set Gizmo color for attack area visualization
            Gizmos.color = Color.green;

            Gizmos.matrix = Matrix4x4.TRS(
                attackPoint,
                CharacterActor.Rotation,
                Vector3.one
            );

            Gizmos.DrawWireCube(Vector3.zero, scaledSize);

            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}
