using UnityEngine;

namespace Actors
{
    public sealed class TargetFollower : MonoBehaviour
    {
        [field: SerializeField] public bool IsEnabled { get; set; } = true;
        [field: SerializeField] public bool DefaultIsRight { get; set; } = true;

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow - X Axis")]
        [SerializeField, Min(0f)] private float followStartDistanceX = 3.0f;
        [SerializeField, Min(0f)] private float followStopDistanceX = 2.0f;

        [Header("Follow - Y Axis")]
        [SerializeField, Min(0f)] private float followStartDistanceY = 1.2f;
        [SerializeField, Min(0f)] private float followStopDistanceY = 0.8f;

        [Header("Follow Motion")]
        [SerializeField, Min(0f)] private float maxFollowSpeed = 6.0f;
        [SerializeField, Min(0.001f)] private float followSmoothTime = 0.15f;

        [Header("Offset")]
        [Tooltip("Target position + Offset 로 추종합니다. (런타임에서는 내부적으로 별도 offset을 사용합니다.)")]
        [SerializeField] private Vector2 followOffset = new(0f, 0f);

        [Header("Facing")]
        [SerializeField, Min(0f)] private float faceDeadzone = 0.01f;

        [Header("Hover")]
        [SerializeField, Min(0f)] private float hoverAmplitude = 0.15f;
        [SerializeField, Min(0f)] private float hoverFrequency = 1.2f;
        [SerializeField] private bool randomizeHoverPhase = true;

        private Transform _targetTransform;
        private bool _isInitialized;

        // Per-axis follow state + velocity
        private bool _followX;
        private bool _followY;
        private float _followVelocityX;
        private float _followVelocityY;

        // Base position (hover applied on top)
        private Vector3 _basePosition;

        // Hover phase
        private float _hoverPhase;

        // Scale handling
        private float _absScaleX;

        // Enabled toggle tracking
        private bool _prevEnabled;

        // Runtime offset handling
        private Vector2 _defaultOffset;
        private Vector2 _runtimeOffset;

        public void Initialize(Transform targetTransform)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _targetTransform = targetTransform;
            target = targetTransform;

            CacheScaleX();

            _defaultOffset = followOffset;
            _runtimeOffset = _defaultOffset;

            SnapBaseToCurrent();
            _prevEnabled = IsEnabled;
        }

        public void SetTarget(Transform targetTransform)
        {
            _targetTransform = targetTransform;
            target = targetTransform;
        }

        private void Awake()
        {
            CacheScaleX();

            _targetTransform = target != null ? target : _targetTransform;

            _defaultOffset = followOffset;
            _runtimeOffset = _defaultOffset;

            SnapBaseToCurrent();

            _hoverPhase = randomizeHoverPhase
                ? Random.Range(0f, Mathf.PI * 2f)
                : 0f;

            _prevEnabled = IsEnabled;
        }

        private void OnEnable()
        {
            _prevEnabled = IsEnabled;
            CacheScaleX();
            SnapBaseToCurrent();
        }

        private void OnValidate()
        {
            if (followStopDistanceX > followStartDistanceX)
                followStopDistanceX = followStartDistanceX;

            if (followStopDistanceY > followStartDistanceY)
                followStopDistanceY = followStartDistanceY;

            if (!Application.isPlaying)
            {
                CacheScaleX();
                _defaultOffset = followOffset;
                _runtimeOffset = _defaultOffset;
            }
        }

        private void Update()
        {
            // 런타임 토글 처리: disable 순간에도 요구사항(오프셋/정렬)을 반영해야 해서, return 이전에 처리합니다.
            if (IsEnabled != _prevEnabled)
            {
                HandleEnabledChanged(isEnabledNow: IsEnabled);
                _prevEnabled = IsEnabled;
            }

            if (!IsEnabled)
                return;

            // Target lazy bind
            if (_targetTransform == null && target != null)
                _targetTransform = target;

            if (_targetTransform != null)
            {
                UpdateFollowingByAxis();
                UpdateFacing();
            }

            ApplyHover();
        }

        /// <summary>
        /// 원하는 위치로 즉시 이동(텔레포트)하고, 지정한 방향을 바라보도록 설정합니다.
        /// </summary>
        public void Teleport(Vector2 targetPosition, bool lookRight)
        {
            CacheScaleX();

            // Follow 상태/속도 초기화
            _followX = false;
            _followY = false;
            _followVelocityX = 0f;
            _followVelocityY = 0f;

            // Base + Transform 동기화
            _basePosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            transform.position = _basePosition;

            // 바라보는 방향 고정
            SetFacing(lookRight);

            // 텔레포트 직후 hover가 튀지 않도록 현재 시점에서 hover를 0으로 맞춤
            ZeroHoverAtCurrentTime();
        }

        private void HandleEnabledChanged(bool isEnabledNow)
        {
            _followX = false;
            _followY = false;
            _followVelocityX = 0f;
            _followVelocityY = 0f;

            if (isEnabledNow)
            {
                // 원래 오프셋 복구
                _runtimeOffset = _defaultOffset;

                // 재활성화 시 '툭' 튀는 느낌 완화
                SnapBaseToCurrent();
                ZeroHoverAtCurrentTime();
                return;
            }

            // 요구사항: IsEnabled == false 가 되면, offset의 y를 'target과 같은 높이'가 되도록 맞춥니다.
            // (followOffset은 보존하고, 런타임에서만 y 오프셋을 0으로 두어 target.y와 동일하게 정렬)
            if (_targetTransform == null && target != null)
                _targetTransform = target;

            if (_targetTransform != null)
            {
                _runtimeOffset.y = 0f;
                _basePosition = transform.position;
                _basePosition.y = _targetTransform.position.y;
                transform.position = _basePosition;
            }
            else
            {
                // 타겟이 없으면 그냥 현재 위치를 베이스로 고정
                SnapBaseToCurrent();
            }
        }

        private void UpdateFollowingByAxis()
        {
            Vector3 desired = _targetTransform.position + new Vector3(_runtimeOffset.x, _runtimeOffset.y, 0f);
            desired.z = _basePosition.z;

            float dx = Mathf.Abs(desired.x - _basePosition.x);
            float dy = Mathf.Abs(desired.y - _basePosition.y);

            // X axis hysteresis
            if (!_followX)
            {
                if (dx >= followStartDistanceX)
                    _followX = true;
            }
            else
            {
                if (dx <= followStopDistanceX)
                    _followX = false;
            }

            // Y axis hysteresis (stricter thresholds)
            if (!_followY)
            {
                if (dy >= followStartDistanceY)
                    _followY = true;
            }
            else
            {
                if (dy <= followStopDistanceY)
                    _followY = false;
            }

            // SmoothDamp per axis
            float newX = _basePosition.x;
            float newY = _basePosition.y;

            if (_followX)
            {
                newX = Mathf.SmoothDamp(
                    _basePosition.x,
                    desired.x,
                    ref _followVelocityX,
                    followSmoothTime,
                    maxFollowSpeed);
            }

            if (_followY)
            {
                newY = Mathf.SmoothDamp(
                    _basePosition.y,
                    desired.y,
                    ref _followVelocityY,
                    followSmoothTime,
                    maxFollowSpeed);
            }

            _basePosition = new Vector3(newX, newY, _basePosition.z);
        }

        private void UpdateFacing()
        {
            float dx = _targetTransform.position.x - _basePosition.x;
            if (Mathf.Abs(dx) < faceDeadzone)
                return;

            SetFacing(lookRight: dx > 0f);
        }

        private void ApplyHover()
        {
            float omega = hoverFrequency * Mathf.PI * 2f;
            float hoverY = Mathf.Sin(Time.time * omega + _hoverPhase) * hoverAmplitude;

            transform.position = _basePosition + new Vector3(0f, hoverY, 0f);
        }

        private void SetFacing(bool lookRight)
        {
            // DefaultIsRight==true: +X 스케일이 오른쪽을 보는 상태
            // DefaultIsRight==false: -X 스케일이 오른쪽을 보는 상태
            float signRight = DefaultIsRight ? 1f : -1f;

            Vector3 s = transform.localScale;
            s.x = _absScaleX * (lookRight ? signRight : -signRight);
            transform.localScale = s;
        }

        private void CacheScaleX()
        {
            _absScaleX = Mathf.Abs(transform.localScale.x);
            if (_absScaleX <= 0f)
                _absScaleX = 1f;
        }

        private void SnapBaseToCurrent()
        {
            _basePosition = transform.position;
        }

        private void ZeroHoverAtCurrentTime()
        {
            float omega = hoverFrequency * Mathf.PI * 2f;
            _hoverPhase = -Time.time * omega;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!IsEnabled)
                return;

            // X thresholds (수평 허용 범위)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(followStartDistanceX * 2f, 0.01f, 0f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(followStopDistanceX * 2f, 0.01f, 0f));

            // Y thresholds (수직 허용 범위)
            Gizmos.color = new Color(1f, 0.6f, 0f);
            Gizmos.DrawWireCube(transform.position, new Vector3(0.01f, followStartDistanceY * 2f, 0f));

            Gizmos.color = new Color(0f, 1f, 1f);
            Gizmos.DrawWireCube(transform.position, new Vector3(0.01f, followStopDistanceY * 2f, 0f));
        }
#endif
    }
}
