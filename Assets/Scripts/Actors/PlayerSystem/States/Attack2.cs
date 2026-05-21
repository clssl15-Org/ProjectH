using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;

namespace Actors.PlayerSystem
{
    public class Attack2 : CharacterState
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
        private Vector2 attackSize = new Vector2(1.0f, 1.0f);

        [SerializeField]
        private Vector2 attackPointOffset = new Vector2(0f, 0f);

        [Header("Attack Properties")]
        private Vector2 attackPoint = Vector2.zero;
        private Vector2 scaledSize = new Vector2(1.0f, 1.0f);
        private float attackAngle = 0f;

        private int attackPower => Player.AttackPower;

        private float attackCursor = 0f;
        private float attackElapsedCursor = 0f;

        private bool comboAvailable = false;
        private bool isDone = true;
        private bool isDamageApplied = false;
        private bool isNextComboReady = false;

        public static Action onAttack2;

        private void TakeDamageToEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                attackPoint,
                scaledSize,
                attackAngle
            );

            foreach (Collider2D hitCollider in hitColliders)
            {
                //print($"2, {hitCollider.name}");

                if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                    continue;

                if(hitCollider.CompareTag("Player"))
                    continue;

                int amount = (int)(attackPower * damageMultiplier);
                damageableObject.TakeDamage(amount);

                onAttack2?.Invoke();
            }
        }
        private void UpdateAttackParameters()
        {
            attackPoint = CharacterActor.ColliderCenter + (new Vector2(attackPointOffset.x * CharacterActor.Forward.x, attackPointOffset.y)) * CharacterActor.Size;
            scaledSize = attackSize * CharacterActor.Size;
            attackAngle = CharacterActor.Rotation.eulerAngles.z;
        }
        public override void CheckExitTransition()
        {
            if (isNextComboReady)
            {
                CharacterStateController.EnqueueTransition<Attack3>();
                return;
            }

            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
                CharacterStateController.AddBufferedState<Attack2>();
            }

            if (CharacterActions.dash.Started)
            {
                CharacterStateController.EnqueueTransition<Dash>();
            }
            
        }
        public override void EnterBehaviour(float dt)
        {
            ResetAttack();
            UpdateAttackParameters();

            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                attackPoint,
                scaledSize,
                attackAngle
            );
            PlayerAction action = PlayerAction.Attack2;
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject) && !hitCollider.CompareTag("Player"))
                    action = PlayerAction.AttackHit2;
            }

            LevelManager.Instance.SoundManager.PlayActionSound(action);
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
                CharacterStateController.RemoveBufferedState<Attack2>();
            }

            if (CharacterActions.attack.Started)
            {
                if (CharacterStateController.CurrentState is not NormalMovement)
                    return;

                CharacterStateController.EnqueueTransition<Attack3>();
                CharacterStateController.RemoveBufferedState<Attack2>();
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
