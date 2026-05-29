using System;
using System.Collections.Generic;
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
        [SerializeField] private bool _useSweepDetection;

        private PlatformManager _platformManager;
        private string[] _exclusionTags = Array.Empty<string>();
        private bool _arrived = false;
        private Collider2D _collider;
        private ContactFilter2D _contactFilter;
        private readonly List<RaycastHit2D> _sweepResults = new();
        private Vector2 _previousPosition;
        private bool _hasPreviousPosition;

        /// <summary>
        /// 안정적인 충돌 처리를 위해 도착 처리를 지연시키는 프레임 수
        /// </summary>
        private const int ArrivalDelay = 10;


        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _contactFilter.useTriggers = true;
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        }

        private void OnEnable()
        {
            ResetSweepPosition();
        }

        public virtual void Initialize(PlatformManager platformManager, params string[] exclusionTags)
        {
            _platformManager = platformManager;
            _exclusionTags = exclusionTags ?? Array.Empty<string>();
        }

        protected virtual void Update()
        {
            if (!_platformManager) _platformManager = PlatformManager.Instance;
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

        private void FixedUpdate()
        {
            DetectSweptArrivals();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            TryArrive(collider);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryArrive(collision.gameObject);
        }

        private void DetectSweptArrivals()
        {
            if (!_useSweepDetection)
                return;

            if (_arrived)
                return;

            if (!_collider)
                _collider = GetComponent<Collider2D>();

            if (!_collider)
                return;

            var currentPosition = (Vector2)_collider.transform.position;
            if (!_hasPreviousPosition)
            {
                SetSweepPosition(currentPosition);
                return;
            }

            var delta = currentPosition - _previousPosition;
            SetSweepPosition(currentPosition);

            if (delta.sqrMagnitude <= Mathf.Epsilon)
                return;

            _sweepResults.Clear();
            var distance = delta.magnitude;
            var count = _collider.Cast(-delta / distance, _contactFilter, _sweepResults, distance);

            for (int i = 0; i < count; i++)
            {
                var hitCollider = _sweepResults[i].collider;
                if (hitCollider && hitCollider != _collider && TryArrive(hitCollider.gameObject))
                    return;
            }
        }

        private bool TryArrive(Collider2D collider) =>
            collider && TryArrive(collider.gameObject);

        private bool TryArrive(GameObject collision)
        {
            if (_arrived)
                return false;

            if (_exclusionTags.Any(collision.CompareTag))
                return false;

            _arrived = true;
            Arrive();
            return true;
        }

        private void ResetSweepPosition()
        {
            if (!_collider)
                _collider = GetComponent<Collider2D>();

            if (_collider)
                SetSweepPosition(_collider.transform.position);
            else
                _hasPreviousPosition = false;
        }

        private void SetSweepPosition(Vector2 position)
        {
            _previousPosition = position;
            _hasPreviousPosition = true;
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
