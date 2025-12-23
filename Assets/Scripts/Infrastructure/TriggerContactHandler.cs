using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Infrastructure
{
    public class TriggerContactHandler : MonoBehaviour
    {
        [Tooltip("이 필드를 할당하지 않으면 자신의 Collider2D 컴포넌트가 자동으로 선택됩니다.")]
        [SerializeField] private Collider2D _myCollider;
        [field: SerializeField] public string[] TargetTags { get; set; }

        public event Action<Collider2D> CollisionEntered;
        public event Action<Collider2D> CollisionExited;
        public IReadOnlyCollection<Collider2D> Collisions => _currentCollisions;

        private ContactFilter2D _contactFilter;

        private readonly List<Collider2D> _overlapResults = new();
        private readonly HashSet<Collider2D> _currentCollisions = new();
        private readonly HashSet<Collider2D> _previousCollisions = new();

        private void Awake()
        {
            if (!_myCollider)
                _myCollider = GetComponent<Collider2D>();

            if (!_myCollider)
            {
                Debug.LogWarning(
                    $"[{nameof(TriggerContactHandler)}] {gameObject.name}: '{nameof(_myCollider)}'이(가) 유효하지 않습니다. ",
                    this);
            }

            _contactFilter.useTriggers = true;
        }

        private void FixedUpdate()
        {
            if (!_myCollider)
                return;

            var count = _myCollider.OverlapCollider(_contactFilter, _overlapResults);

            _currentCollisions.Clear();
            for (int i = 0; i < count; i++)
            {
                var collision = _overlapResults[i];
                if (collision && IsTarget(collision))
                    _currentCollisions.Add(collision);
            }

            foreach (var collision in _currentCollisions)
            {
                if (_previousCollisions.Contains(collision))
                    continue;

                collision
                    .GetOrAddComponent<DestroyEventHandler>()
                    .Register((this, collision), () =>
                    {
                        _previousCollisions.Remove(collision);
                        CollisionExited?.Invoke(collision);
                    });

                CollisionEntered?.Invoke(collision);
            }

            foreach (var collision in _previousCollisions)
            {
                if (_currentCollisions.Contains(collision) || !IsTarget(collision))
                    continue;

                if (collision.TryGetComponent<DestroyEventHandler>(out var destHandler))
                    destHandler.Remove((this, collision));

                CollisionExited?.Invoke(collision);
            }

            _previousCollisions.Clear();
            _previousCollisions.UnionWith(_currentCollisions);
        }

        private void OnDisable()
        {
            foreach (var collision in _previousCollisions)
            {
                if (collision.TryGetComponent<DestroyEventHandler>(out var destHandler))
                    destHandler.Remove((this, collision));

                CollisionExited?.Invoke(collision);
            }

            _previousCollisions.Clear();
        }

        private bool IsTarget(Collider2D collision)
        {
            if (TargetTags == null || TargetTags.Length == 0)
                return true;

            return TargetTags.Any(tag => collision.CompareTag(tag));
        }
    }
}
