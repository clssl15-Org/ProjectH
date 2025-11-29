using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteSizeHandler))]
    [RequireComponent(typeof(Rigidbody2D), typeof(Projectile))]
    public class FallingStone : MonoBehaviour
    {
        public FallingStone Initialize(
            Configuration configuration,
            PlatformManager platformManager,
            float gravitySacle)
        {
            GetComponent<SpriteSizeHandler>()
                .Initialize(configuration)
                .RequestApplyScaleFactor();

            GetComponent<Rigidbody2D>().gravityScale = gravitySacle;
            GetComponent<Projectile>().Initialize(platformManager);

            return this;
        }
    }
}
