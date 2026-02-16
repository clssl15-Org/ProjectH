using System;
using System.Linq;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }

        public bool HasArrived { get; private set; } = false;
        public float Tolerance
        {
            get => _tolerance;
            set => _tolerance = Mathf.Max(0, value);
        }

        [SerializeField, Min(0)] private float _tolerance = 1f;

        private PlatformManager _platformManager;
        private string[] _collisionTags;
        private bool _arrived = false;

        /// <summary>
        /// 안정적인 충돌 처리를 위해 도착 처리를 지연시키는 프레임 수
        /// </summary>
        private const int ArrivalDelay = 10;


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
                Debug.LogError($"[Projectile] {nameof(PlatformManager)}이(가) 없기 때문에 투사체 '{name}'을(를) 사용할 수 없습니다.", this);
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

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_arrived
                || _collisionTags == null
                || _collisionTags.Length == 0)
                return;

            if (_collisionTags.Any(t => collider.gameObject.CompareTag(t)))
            {
                _arrived = true;
                Arrive();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_arrived
                || _collisionTags == null
                || _collisionTags.Length == 0)
                return;

            if (_collisionTags.Any(t => collision.gameObject.CompareTag(t)))
            {
                _arrived = true;
                Arrive();
            }
        }

        private void Arrive()
        {
            // 안정적인 대미지 처리를 위해 지연된 프레임에 도착 처리 실행
            IDisposable handle = null;
            int frameCount = ArrivalDelay;

            handle = Loco.Subscribe(() =>
            {
                frameCount--;
                if (frameCount > 0) return;

                handle.Dispose();

                HasArrived = true;
                OnArrived();
            });
        }

        public virtual void OnArrived()
        {
            if (this) Destroy(gameObject);
        }
    }
}
