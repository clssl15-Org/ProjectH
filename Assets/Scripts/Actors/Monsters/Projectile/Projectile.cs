using System.Linq;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }

        public float Tolerance
        {
            get => _tolerance;
            set => _tolerance = Mathf.Max(0, value);
        }

        [SerializeField, Min(0)] private float _tolerance = 1f;

        private PlatformManager _platformManager;
        private string[] _collisionTags;


        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

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

            var min = _platformManager.Bounds.min;
            var max = _platformManager.Bounds.max;

            if (transform.position.x < min.x - _tolerance
                || transform.position.x > max.x + _tolerance
                || transform.position.y < min.y - _tolerance
                || transform.position.y > max.y + _tolerance)
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
