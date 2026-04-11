using System;
using UnityEngine;
using Infrastructure;

namespace World
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

        /// <summary>
        /// 플랫폼을 탐지할 때 하단으로 내려가는 간격(해상도)입니다.
        /// 값이 작을수록 촘촘하게 검사하여 얇은 플랫폼도 탐지할 수 있습니다.
        /// </summary>
        public float DetectionStep
        {
            get => _detectionStep;
            set => _detectionStep = Mathf.Max(0.01f, value); // 0 이하 방지
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

        [Tooltip("탐지 간격입니다. 얇은 플랫폼 두께보다 작은 값으로 설정해야 탐지 가능합니다.")]
        [SerializeField, Min(0.01f)] private float _detectionStep = 0.1f; // 기본값을 0.1로 두어 촘촘하게 설정

        // Internal
        private Collider2D SelfCollider => _selfCollider ??= GetComponent<Collider2D>();
        private Collider2D _selfCollider;


        // Content
        public void SetPlatformManager(PlatformManager platformManager) => _platformManager = platformManager;
        public PlatformManager GetPlatformManager() => _platformManager;

        public bool TryGetCurrentPlatformId(out int platformId) => TryGetPlatformId(Bottom, out platformId);

        public bool TryGetPlatformId(Vector3 position, out int platformId)
        {
            platformId = -1;

            if (!_platformManager)
                throw new InvalidOperationException(
                    $"{GetType().Name} 객체를 사용하려면 {nameof(_platformManager)}이(가) 할당되어 있어야 합니다.");

            // 설정한 간격(Step)이 너무 작아 무한 루프가 도는 것을 방지
            float safeStep = Mathf.Max(0.01f, _detectionStep);

            // 전체 탐지 높이를 탐지 간격으로 나누어 총 몇 번 검사할지 계산
            var steps = Mathf.CeilToInt((_detectionHeight + Mathf.Epsilon) / safeStep);

            // 기존처럼 cellHeight/2를 빼지 않고, 기준 위치부터 촘촘하게 내려가며 검사
            var p = position;

            for (int i = 0; i <= steps; i++) // <= 를 사용하여 최대 높이까지 확실히 포함
            {
                if (_platformManager.TryGetPlatformId(p, out platformId))
                    return true;

                p.y -= safeStep;
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
