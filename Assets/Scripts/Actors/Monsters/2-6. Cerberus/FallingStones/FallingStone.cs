using System;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteSizeHandler))]
    [RequireComponent(typeof(Rigidbody2D), typeof(Projectile))]
    public class FallingStone : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _scale = 1f;

        public event Action GroundReached;
        private bool _hasReachedGround;

        public FallingStone Initialize(
            Configuration configuration,
            PlatformManager platformManager,
            float gravityScale,
            params string[] exclusionTags)
        {
            GetComponent<SpriteSizeHandler>()
                .Initialize(configuration)
                .RequestApplyScaleFactor();

            GetComponent<Rigidbody2D>().gravityScale = gravityScale;
            GetComponent<Projectile>().Initialize(platformManager, exclusionTags);

            transform.localScale = Vector3.one * _scale;
            return this;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_hasReachedGround)
                return;
            
            if (collider.CompareTag("Ground"))
            {
                _hasReachedGround = true;
                GroundReached?.Invoke();
            }
        }
    }
}