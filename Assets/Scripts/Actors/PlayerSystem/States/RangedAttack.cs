using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class RangedAttack : CharacterState
    {
        [Header("Skill Timing Settings")]
        [SerializeField]
        private float motionDuration = 1f;

        [SerializeField]
        private float launchDelay = 0.1f;

        [Header("Projectile Settings")]
        [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField]
        private bool isSizeSynced = true;

        [SerializeField]
        private Vector2 offset;

        [Header("Attack Stats")]
        [SerializeField]
        private float damageMultiplier = 3f;

        [Header("Cooldown Settings")]
        [SerializeField]
        private float cooldownDuration = 5f;

        private int attackPower => Player.playerStats.attackPower;
        private float SkillCooldownMultiplier => Player.playerStats.skillCooldownMultiplier;

        private float skillCursor = 0f;

        private bool isDone = true;
        private bool isProjectileLaunched = true;

        private CooldownTimer cooldownTimer;
        public float BonusMultiplier { get; set; } = 1f;

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
            ResetSkill();
            cooldownTimer = gameObject.AddComponent<CooldownTimer>();
            cooldownTimer.StartCooldown(cooldownDuration * SkillCooldownMultiplier, dt);

            LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.RangedAttack);
        }

        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / motionDuration;
            skillCursor += animationDt;

            if (skillCursor >= 1f)
            {
                isDone = true;
            }

            if (skillCursor >= launchDelay && !isProjectileLaunched)
            {
                Vector2 position = CharacterActor.ColliderCenter + offset;
                Vector2 direction = CharacterActor.Forward;
                Quaternion rotation = CharacterActor.Rotation;

                GameObject newProjectile = Instantiate(projectilePrefab, position, rotation);
                newProjectile.GetComponent<ProjectileMovement>().ResetProjectile(dt, direction);
                newProjectile.GetComponent<ProjectileDamage>().Damage = (int)(attackPower * damageMultiplier * BonusMultiplier);
                newProjectile.GetComponent<SpriteRenderer>().flipX = CharacterActor.Forward.x < 0 ? true : false;
                if (isSizeSynced)
                    newProjectile.transform.localScale = CharacterActor.transform.localScale;

                isProjectileLaunched = true;
            }
        }

        private void ResetSkill()
        {
            skillCursor = 0f;
            isDone = false;
            isProjectileLaunched = false;
        }
    }
}
