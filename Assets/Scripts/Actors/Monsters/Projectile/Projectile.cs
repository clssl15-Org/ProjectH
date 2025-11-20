using System.Linq;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private PlatformManager _platformManager;
        private string[] _collisionTags;


        public virtual void Initialize(PlatformManager platformManager, params string[] collisionTags)
        {
            _platformManager = platformManager;
            _collisionTags = collisionTags;
        }

        protected virtual void Update()
        {
            if (!_platformManager)
            {
                Debug.LogError($"[Projectile] PlatformManager가 없기 때문에 투사체 {name}을(를) 사용할 수 없습니다.");
                Destroy(gameObject);
                return;
            }

            var min = _platformManager.Bound.min;
            var max = _platformManager.Bound.max;

            if (transform.position.x < min.x || transform.position.x > max.x
                || transform.position.y < min.y || transform.position.y > max.y)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (_collisionTags == null || _collisionTags.Length == 0)
                return;

            if (_collisionTags.Any(t => collision.gameObject.CompareTag(t)))
                OnArrived();
        }

        public virtual void OnArrived()
        {
            Destroy(gameObject);
        }
    }
}
