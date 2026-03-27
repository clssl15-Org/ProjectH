using Infrastructure;
using UnityEngine;
using World;
using System;

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteSizeHandler))]
    [RequireComponent(typeof(Rigidbody2D), typeof(Projectile))]
    public class FallingStone : MonoBehaviour
    {
        public event Action GroundReached;
        private bool _hasReachedGround;

        public FallingStone Initialize(
            Configuration configuration,
            PlatformManager platformManager,
            float gravityScale,
            params string[] collisionTags)
        {
            GetComponent<SpriteSizeHandler>()
                .Initialize(configuration)
                .RequestApplyScaleFactor();

            GetComponent<Rigidbody2D>().gravityScale = gravityScale;
            GetComponent<Projectile>().Initialize(platformManager, collisionTags);

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