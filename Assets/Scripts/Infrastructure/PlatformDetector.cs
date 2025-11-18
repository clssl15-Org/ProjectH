using System;
using UnityEngine;

namespace Infrastructure
{
    [RequireComponent(typeof(Collider2D))]
    public class PlatformDetector : MonoBehaviour
    {
        // Front
        /// <summary>
        /// 객체의 기준 위치가 플랫폼으로부터 해당 높이까지의 위치 안에 있으면 플랫폼에 존재한다고 가정합니다.
        /// </summary>
        public float DetectionHeight
        {
            get => _detectionHeight;
            set => _detectionHeight = value;
        }
        /// <summary>
        /// 객체의 기준 위치에서 해당 범위에 해당하는 부분의 플랫폼을 탐지합니다.
        /// </summary>
        public float DetectionRange
        {
            get => _detectionRange;
            set => _detectionRange = value;
        }

        // Property
        /// <summary>
        /// 객체의 기준 위치입니다.
        /// </summary>
        public Vector3 Bottom => new Vector3
        {
            x = transform.position.x,
            y = SelfCollider.bounds.min.y,
            z = transform.position.z
        };

        [SerializeField] private PlatformManager _platformManager;
        [SerializeField, Min(0)] private float _detectionHeight = 1f;
        [SerializeField, Min(0)] private float _detectionRange = 1f;

        // Internal
        private Collider2D SelfCollider => _selfCollider ??= GetComponent<Collider2D>();
        private Collider2D _selfCollider;


        // Content
        public void SetPlatformManager(PlatformManager platformManager) => _platformManager = platformManager;

        public bool TryGetCurrentPlatformId(out int platformId) => TryGetPlatformId(Bottom, out platformId);
        public bool TryGetPlatformId(Vector3 position, out int platformId)
        {
            platformId = -1;

            if (!_platformManager)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} 객체를 사용하려면 {nameof(_platformManager)}이(가) 할당되어 있어야 합니다.");
            }

            var cellHeight = _platformManager.CellSize.y;
            var steps = Mathf.FloorToInt((_detectionHeight + Mathf.Epsilon) / cellHeight);

            var p = position - new Vector3(0, cellHeight / 2f, 0);
            for (int i = 0; i < steps; i++)
            {
                if (_platformManager.TryGetPlatformId(p, out platformId))
                    return true;

                p.y -= cellHeight;
            }

            return false;
        }


        /// <summary>
        /// 지정한 방향의 위치에서 현재 플랫폼이 platformId와 일치하는지 확인합니다.
        /// </summary>
        public bool CheckPlatform(Direction direction, int expectedPlatformId, out int detectedPlatformId)
        {
            if (expectedPlatformId < 0)
            {
                detectedPlatformId = -1;
                return false;
            }

            var offsetX = direction switch
            {
                Direction.Left => -_detectionRange,
                Direction.Right => _detectionRange,
                _ => 0f
            };

            var position = Bottom + new Vector3(offsetX, 0f, 0f);
            return TryGetPlatformId(position, out detectedPlatformId) && detectedPlatformId == expectedPlatformId;
        }
    }
}
