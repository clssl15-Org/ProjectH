using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class UltimateProjectileMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Min(0f)]
        [SerializeField]
        protected float initialVelocity = 12f;

        [Min(0f)]
        [SerializeField]
        private float duration = 1f;

        private float dt;
        private float projectileCursor;
        private Vector2 direction = Vector2.right;

        private CircleCollider2D circleCol;

        private void Awake()
        {
            circleCol = GetComponent<CircleCollider2D>();
        }
        private void Update()
        {
            float dt = Time.deltaTime;

            float animationDt = dt / duration;
            projectileCursor += animationDt;

            if (projectileCursor >= 1f)
            {
                Destroy(gameObject);
            }

            
        }
        public void GrowRadius(float targetRadius)
        {
            StartCoroutine(GrowRoutine(targetRadius));
        }
        IEnumerator GrowRoutine(float targetRadius)
        {
            float startRadius = circleCol.radius;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // float 값 보간에는 Mathf.Lerp를 사용합니다.
                circleCol.radius = Mathf.Lerp(startRadius, targetRadius, elapsed / duration);
                yield return null;
            }

            circleCol.radius = targetRadius;
        }
        public void ResetProjectile(float dt, Vector2 direction)
        {
            this.dt = dt;
            this.direction = direction;
            projectileCursor = 0f;
        }
    }
}