using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;

namespace Actors.PlayerSystem
{
    public class Attack3 : CharacterState
    {
        [Header("Attack Timing Settings")]
        // The duration of the attack animation
        [SerializeField]
        private float attackDuration = 1f;

        [SerializeField]
        private float damageApplyTime = 0.3f;

        [Header("Attack Stats")]
        [SerializeField]
        private float damageMultiplier = 1.0f;

        [Header("Attack Range")]
        [SerializeField]
        private Vector2 attackSize = new Vector2(1.0f, 0.5f);

        [SerializeField]
        private Vector2 attackPointOffset = new Vector2(0f, 0f);

        [Header("Attack Properties")]
        private Vector2 attackPoint = Vector2.zero;
        private Vector2 scaledSize = new Vector2(1.0f, 1.0f);
        private float attackAngle = 0f;

        private float attackPower => Player.playerStats.attackPower;

        private float attackCursor = 0f;

        //private bool comboAvailable = false;
        private bool isDone = true;
        private bool isDamageApplied = false;

        public static Action onAttack3;

        private void TakeDamageToEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                attackPoint,
                scaledSize,
                attackAngle
            );

            foreach (Collider2D hitCollider in hitColliders)
            {
                //print($"3, {hitCollider.name}");

                if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
                    continue;

                if (hitCollider.CompareTag("Player"))
                    continue;

                int amount = (int)(attackPower * damageMultiplier);
                damageableObject.TakeDamage(amount);
                onAttack3?.Invoke();
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
            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
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

            LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.Attack3);
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
        }

        public override void ExitBehaviour(float dt)
        {
            isDone = true;
        }

        private void ResetAttack()
        {
            attackCursor = 0f;
            //comboAvailable = false;
            isDone = false;
            isDamageApplied = false;
        }

        private void OnDrawGizmos()
        {
            if (isDone) return;

            if (CharacterActor == null) return;

            Gizmos.color = Color.green;

            Gizmos.matrix = Matrix4x4.TRS(
                attackPoint,
                CharacterActor.Rotation,
                Vector3.one
            );

            Gizmos.DrawWireCube(Vector3.zero, scaledSize);

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
