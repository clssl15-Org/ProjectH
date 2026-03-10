using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Actors.PlayerSystem;

namespace Actors.PlayerSystem
{
    public class Ultimate : CharacterState
    {
        [SerializeField]
        RuntimeAnimatorController nextRuntimeAnimatorController = null;
        public RuntimeAnimatorController NextRuntimeAnimatorController => nextRuntimeAnimatorController;

        [Header("Skill Timing Settings")]
        [SerializeField]
        private float skillDuration = 1f;

        [SerializeField]
        private float cancleTime = 0.8f;

        [SerializeField]
        private float auraEffectDestroyTiming;

        [Header("Effect Settings")]
        [SerializeField]
        private GameObject auraEffectPrefab;
        [SerializeField]
        private GameObject projectilePrefab;
        [SerializeField]
        private float targetRadius;

        [SerializeField]
        private DirectionMode directionMode = DirectionMode.InputDirection;

        [Header("Attack Stats")]
        [SerializeField]
        private float damageMultiplier = 1.0f;

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

        [Header("Attack Properties")]
        private Vector2 attackPoint = Vector2.zero;
        private Vector2 scaledSize = new Vector2(1.0f, 1.0f);
        private float attackAngle = 0f;

        [Header("Cooldown Settings")]
        [SerializeField]
        private float cooldownRecoveryAmount = 0.02f;

        public float CooldownGauge => cooldownGauge;
        private float cooldownGauge = 1f;

        [Header("Gizmos")]
        private bool isHitBoxEnabled = false;

        private float attackPower => Player.playerStats.attackPower;
        private float skillPowerMultiflier => Player.playerStats.skillPowerMultiplier;
        private float ultimateCooldownMultiplier => Player.playerStats.ultimateCooldownMultiplier;

        private float skillCursor = 0f; 
        private float currentSkillTime = 0f;

        private Vector2 direction = Vector2.right;

        private bool isCanceled = false;
        private bool isDone = true;
        private bool isProjectileLaunched = false;
        private bool isAuraEffectDestroyed = false;

        private GameObject auraEffect;

        private void OnEnable()
        {
            Attack1.onAttack1 += RecoverCooldown;
            Attack2.onAttack2 += RecoverCooldown;
            Attack3.onAttack3 += RecoverCooldown;
            RushStabbing.onEskill += RecoverCooldown;
            ProjectileDamage.onRangedAttack += RecoverCooldown;
        }
        private void OnDisable()
        {
            Attack1.onAttack1 -= RecoverCooldown;
            Attack2.onAttack2 -= RecoverCooldown;
            Attack3.onAttack3 -= RecoverCooldown;
            RushStabbing.onEskill -= RecoverCooldown;
            ProjectileDamage.onRangedAttack -= RecoverCooldown;
        }

        public override bool CheckEnterTransition(CharacterState fromState)
        {
            if (cooldownGauge < 1f)
            {
                Debug.Log(cooldownGauge);
                return false;
            }

            return true;
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

            auraEffect = Instantiate(auraEffectPrefab, CharacterActor.Position, CharacterActor.Rotation);
            LevelManager.Instance.SoundManager.ResetLoopSound();
            LevelManager.Instance.SoundManager.PlayLoopSound(PlayerAction.UltimateCharge);

            ResetSkill();
        }

        public override void UpdateBehaviour(float dt)
        {
            currentSkillTime += dt;

            if (CharacterActions.ultimate.Canceled)
            {
                isCanceled = true;
            }

            if (isCanceled && currentSkillTime < cancleTime)
            {
                print(currentSkillTime);
                isDone = true;
                Destroy(auraEffect);
                return;
            }

            if (isCanceled && !isProjectileLaunched)
            {
                Vector2 position = CharacterActor.Position + (attackPointOffset * CharacterActor.Forward);
                Vector2 direction = CharacterActor.Forward;
                Quaternion rotation = CharacterActor.Rotation;
                if (projectilePrefab != null)
                {
                    GameObject newProjectile = Instantiate(projectilePrefab, position, rotation);
                    newProjectile.GetComponent<UltimateProjectileMovement>().ResetProjectile(dt, direction);
                    newProjectile.GetComponent<UltimateProjectileMovement>().GrowRadius(targetRadius);
                    newProjectile.GetComponent<ProjectileDamage>().Damage = (int)(attackPower * damageMultiplier * skillPowerMultiflier);
                    newProjectile.GetComponent<SpriteRenderer>().flipX = CharacterActor.Forward.x < 0 ? true : false;

                }

                Destroy(auraEffect);
                cooldownGauge = 0f;
                isProjectileLaunched = true;
                Player.Invincible = true;
                CharacterActor.Animator.runtimeAnimatorController = nextRuntimeAnimatorController;
                LevelManager.Instance.SoundManager.StopLoopSound();
                LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.UltimateRelease);
            }

            if (isProjectileLaunched)
            {
                float animationDt = dt / skillDuration;
                skillCursor += animationDt;

                if (skillCursor > 1)
                {
                    isDone = true;
                    skillCursor = 0f;
                }
            }
        }

        public override void ExitBehaviour(float dt)
        {
            Player.Invincible = false;
        }

        private void ResetSkill()
        {
            isDone = false;
            isCanceled = false;
            isProjectileLaunched = false;
            isAuraEffectDestroyed = false;
            skillCursor = 0;
            currentSkillTime = 0f;
        }

        private void RecoverCooldown()
        {
            cooldownGauge += cooldownRecoveryAmount * ultimateCooldownMultiplier;
            cooldownGauge = Mathf.Clamp(cooldownGauge, 0f, 1f);
        }
    }
}
