using System;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(MonsterAudioPlayer))]
    public class VerbelionPortalAttackSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _effect;
        [SerializeField] private KinematicProjectile _projectilePrefab;
        [SerializeField, Min(0.01f)] private float _projectileSpeed = 1f;
        [SerializeField, Min(1)] private int _targetFireCount = 3;
        [SerializeField, Min(0)] private float _projectileFireGap = 1f;

        [Header("Bindings")]
        [SerializeField] private Configuration _configuration;
        [SerializeField] private PlatformManager _platformManager;

        private MonsterAudioPlayer _audioPlayer;

        private Action<KinematicProjectile>[] _initializers;
        private Func<Vector2> _getTargetPosition;
        private int _firedCount;
        private float _remainingTime;

        [Header("Debug")]
        [SerializeField] private Transform _target;


        private void Awake() => _audioPlayer = GetComponent<MonsterAudioPlayer>();

        public VerbelionPortalAttackSpawner Initialize(
            PlatformManager platformManager,
            Func<Vector2> getTargetPosition)
        {
            _platformManager = platformManager;
            _getTargetPosition = getTargetPosition;

            return this;
        }

        public VerbelionPortalAttackSpawner SetInitializer(params Action<KinematicProjectile>[] initializers)
        {
            _initializers = initializers;
            return this;
        }

        public void RequestStart() => gameObject.SetActive(true);

        private void OnEnable()
        {
            if (!_projectilePrefab)
                throw new InvalidOperationException(
                    $"{nameof(VerbelionPortalAttackSpawner)}은(는) {nameof(_projectilePrefab)}을(를) 가지고 있어야 합니다.");

            if (!_platformManager)
                throw new InvalidOperationException(
                    $"{nameof(VerbelionPortalAttackSpawner)}은(는) {nameof(_platformManager)}을(를) 가지고 있어야 합니다.");

            if (_getTargetPosition == null && !_target)
                throw new InvalidOperationException(
                    $"{nameof(VerbelionPortalAttackSpawner)}은(는) {nameof(_getTargetPosition)} 혹은 {nameof(_target)} " +
                    $"둘 중 하나를 가지고 있어야 합니다.");

            if (_configuration)
                foreach (var ssh in GetComponentsInChildren<SpriteSizeHandler>())
                    ssh.Initialize(_configuration, true);

            _firedCount = 0;
            _remainingTime = 0;
        }

        private void Update()
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0) return;

            _remainingTime = _projectileFireGap;

            if (_firedCount >= _targetFireCount)
            {
                gameObject.SetActive(false);
                return;
            }

            _firedCount++;

            _effect.SetActive(false);
            _effect.SetActive(true);

            var projectile = Instantiate(_projectilePrefab).GetComponent<KinematicProjectile>();
            projectile.transform.position = transform.position;
            projectile.Initialize(_platformManager);

            if (_initializers != null)
            {
                foreach (var initializer in _initializers)
                    initializer?.Invoke(projectile);
            }
            projectile.gameObject.SetActive(true);

            var dir =
                (_getTargetPosition?.Invoke() ?? (Vector2)_target.position)
                - (Vector2)transform.position;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.down;

            var targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            projectile.transform.rotation = Quaternion.Euler(0, 0, targetAngle);

            projectile.Launch(dir, _projectileSpeed);
            _audioPlayer.Play("PortalAttack");
        }

        public void RequestStop() => _firedCount = int.MaxValue;
    }
}
