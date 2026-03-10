using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Rules;

namespace Actors.PlayerSystem
{
    public class Attack1 : CharacterState
    {
        [Header("Attack Timing Settings")]
        // The duration of the attack animation
        [SerializeField]
        private float attackDuration = 1f;

        [SerializeField]
        private float damageApplyTime = 0.3f;

        // Time window to allow for combo attacks
        [SerializeField]
        private float comboTimeWindow = 0.3f;

        // Time before the next combo can be initiated
        [SerializeField]
        private float nextComboTime = 0.5f;

        // Duration allowed for chaining the next combo input
        [SerializeField]
        private float comboInputTimeWindow = 0.7f;

        [Header("Attack Stats")]
        [SerializeField]
        private float damageMultiplier = 1.0f;

        [Header("Attack Range")]
        [SerializeField]
        private float attackRange = 1.0f;

        [SerializeField]
        private float attackAngle = 90f;

        [SerializeField]
        private Vector2 attackPointOffset = new Vector2(0f, 0f);

        [Header("Attack Properties")]
        private Vector2 attackPoint = Vector2.zero;
        private float scaledSize = 1.0f;

        private float attackPower => Player.playerStats.attackPower;

        private float attackCursor = 0f;
        private float attackElapsedCursor = 0f;

        private bool comboAvailable = false;
        private bool isDone = true;
        private bool isDamageApplied = false;
        private bool isNextComboReady = false;

        public static Action onAttack1;

        private void TakeDamageToEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                attackPoint,
                scaledSize
            );

            //Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            //    attackPoint,
            //    scaledSize * Vector2.one,
            //    0
            //);

            foreach (Collider2D hitCollider in hitColliders)
            {
                //print($"1, {hitCollider.name}");

                if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                    continue;

                if (hitCollider.CompareTag("Player"))
                {
                    continue;
                }

                Vector2 directionToEnemy = ((Vector2)hitCollider.transform.position - attackPoint).normalized;
                float angle = Vector2.Angle(CharacterActor.Forward, directionToEnemy);

                if (angle <= attackAngle)
                {
                    int amount = (int)(attackPower * damageMultiplier);
                    damageableObject.TakeDamage(amount);
                    
                    onAttack1?.Invoke();
                }
            }
        }
        private void UpdateAttackParameters()
        {
            attackPoint = CharacterActor.ColliderCenter + (attackPointOffset * CharacterActor.Size);
            scaledSize = attackRange * CharacterActor.Size;
        }

        public override void CheckExitTransition()
        {
            if (isNextComboReady)
            {
                CharacterStateController.EnqueueTransition<Attack2>();
                return;
            }

            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
                CharacterStateController.AddBufferedState<Attack1>();
            }

            if (CharacterActions.dash.Started)
            {
                CharacterStateController.EnqueueTransition<Dash>();
            }

            if (CharacterActions.changeSkill.Started)
            {
                //CharacterStateController.EnqueueTransition<Eskill>();
            }
        }
        public override void EnterBehaviour(float dt)
        {
            //CharacterActor.Velocity = new Vector2(0, 0);

            ResetAttack();
            UpdateAttackParameters();

            LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.Attack1);
        }

        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / attackDuration;
            attackCursor += animationDt;

            if (attackCursor >= 1f)
            {
                isDone = true;
            }

            if (attackCursor >= damageApplyTime && !isDamageApplied)
            {
                isDamageApplied = true;
                TakeDamageToEnemy();
            }

            if (!CharacterActions.attack.Started)
                return;

            if (attackCursor >= comboTimeWindow)
            {
                comboAvailable = true;
            }

            if (attackCursor >= nextComboTime && comboAvailable)
            {
                isDone = true;
                isNextComboReady = true;
            }
        }

        public override void UpdateBufferedActions(float dt)
        {
            float animationDt = dt / comboInputTimeWindow;
            attackElapsedCursor += animationDt;

            if (attackElapsedCursor >= 1f)
            {
                CharacterStateController.RemoveBufferedState<Attack1>();
            }

            if (CharacterActions.attack.Started)
            {
                if (CharacterStateController.CurrentState is not NormalMovement)
                    return;

                CharacterStateController.EnqueueTransition<Attack2>();
                CharacterStateController.RemoveBufferedState<Attack1>();
            }
        }

        public override void ExitBehaviour(float dt)
        {
            isDone = true;
        }

        private void ResetAttack()
        {
            attackCursor = 0f;
            attackElapsedCursor = 0f;
            comboAvailable = false;
            isDone = false;
            isDamageApplied = false;
            isNextComboReady = false;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (isDone) return;

            if (CharacterActor == null) return;

            // Draw attack center point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(attackPoint, 0.1f);

            // Visualize partial attack range
            Gizmos.color = Color.green;

            // Number of segments dividing the half circle
            int segments = 30;

            // Half of the field of view angle
            float halfFOV = attackAngle;

            Vector2 forward = CharacterActor.Forward.normalized;

            for (int i = 0; i <= segments; i++)
            {
                float angleA = -halfFOV + (i * (halfFOV * 2) / segments);
                float angleB = -halfFOV + ((i + 1) * (halfFOV * 2) / segments);

                Vector2 dirA = RotateVector(forward, angleA);
                Vector2 dirB = RotateVector(forward, angleB);

                Vector2 pointA = attackPoint + dirA * scaledSize;
                Vector2 pointB = attackPoint + dirB * scaledSize;

                Gizmos.DrawLine(attackPoint, pointA);
                Gizmos.DrawLine(pointA, pointB);
            }
        }

        Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
#endif
    }
}
