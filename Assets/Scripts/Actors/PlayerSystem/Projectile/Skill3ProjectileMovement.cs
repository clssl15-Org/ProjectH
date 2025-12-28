using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class Skill3ProjectileMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Min(0f)]
        [SerializeField]
        protected float initialVelocity = 12f;

        [Min(0f)]
        [SerializeField]
        private float duration = 1f;

        [SerializeField]
        protected AnimationCurve movementCurve = AnimationCurve.Linear(0, 1, 1, 0);

        

        private float dt;
        private float projectileCursor;
        private Vector2 direction = Vector2.right;

        private void Update()
        {
            // TODO: 여기 임시로 고침 - dt
            float dt = Time.deltaTime;

            float animationDt = dt / duration;
            projectileCursor += animationDt;

            if (projectileCursor >= 1f)
            {
                Destroy(gameObject);
            }

            Vector2 proVelocity = initialVelocity * movementCurve.Evaluate(projectileCursor) * direction * dt;
            transform.Translate(proVelocity);
        }

        public void ResetProjectile(float dt, Vector2 direction)
        {
            this.dt = dt;
            this.direction = direction;
            projectileCursor = 0f;
        }
    }
}
