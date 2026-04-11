using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class Skill3 : CharacterState
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
        private Vector2 offset = Vector2.zero;


        [Header("Attack Stats")]
        [SerializeField]
        private float damageMultiplier = 3f;

        [Header("Cooldown Settings")]
        [SerializeField]
        private float cooldownDuration = 5f;

        [Header("Invincible Timer")]
        [SerializeField]
        private float invincibleStartTime = 0f;
        [SerializeField]
        private float invincibleEndTime = 0.5f;

        private float attackPower => Player.playerStats.attackPower;
        private float skillPowerMultiflier => Player.playerStats.skillPowerMultiplier;
        private float SkillCooldownMultiplier => Player.playerStats.skillCooldownMultiplier;

        private float skillCursor = 0f;

        private bool isDone = true;
        private bool isProjectileLaunched = true;
        public float BonusMultiplier { get; set; } = 1f;

        private CooldownTimer cooldownTimer;

        public override bool CheckEnterTransition(CharacterState fromState)
        {
            return !cooldownTimer || !cooldownTimer.IsOnCooldown;
        }
        public override void CheckExitTransition()
        {
            if (isDone)
            {
                CharacterStateController.EnqueueTransition<NormalMovement>();
                Player.ResetRandomSkillBuff();
            }
        }

        public override void EnterBehaviour(float dt)
        {
            ResetSkill();
            cooldownTimer = gameObject.AddComponent<CooldownTimer>();
            cooldownTimer.CooldownType = CooldownType.Skill3;
            cooldownTimer.OnCooldownStart += () => {
            };
            cooldownTimer.OnCooldownComplete += () =>
            {
            };
            cooldownTimer.StartCooldown(cooldownDuration * SkillCooldownMultiplier, dt);
            LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.Skill3);
        }

        public override void UpdateBehaviour(float dt)
        {
            float animationDt = dt / motionDuration;
            skillCursor += animationDt;

            if (skillCursor >= 1f)
            {
                isDone = true;
            }

            if (skillCursor >= invincibleStartTime && skillCursor <= invincibleEndTime)
            {
                Player.Invincible = true;
            }
            else
            {
                Player.Invincible = false;
            }

            if (skillCursor >= launchDelay && !isProjectileLaunched)
            {
                Vector2 position = CharacterActor.Position + (offset * CharacterActor.Forward);
                Vector2 direction = CharacterActor.Forward;
                Quaternion rotation = CharacterActor.Rotation;

                GameObject newProjectile = Instantiate(projectilePrefab, position, rotation);
                newProjectile.GetComponent<Skill3ProjectileMovement>().ResetProjectile(dt, direction);
                newProjectile.GetComponent<ProjectileDamage>().Damage = Player.CalculateDamage((int)(attackPower * damageMultiplier * BonusMultiplier * skillPowerMultiflier));
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
