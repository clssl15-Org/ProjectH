using System;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    public enum KinematicProjectileLaunchType
    {
        Rotation,
        LocalRotation,
        Directions,
    }

    internal class KinematicProjectileLauncher : MonoBehaviour
    {
        // Property
        [SerializeField] private GameObject[] _projectiles;

        // Internal
        private Vector3[] _projectilePositions;
        private Action<KinematicProjectile>[] _projectileInitializers;

        private IMonsterInternal _owner;
        private PlatformManager _platformManager;
        private string[] _collisionTags;


        // Content
        private void Start()
        {
            _projectilePositions = new Vector3[_projectiles.Length];

            for (int i = 0; i < _projectiles.Length; i++)
            {
                _projectilePositions[i] = _projectiles[i].transform.localPosition;
                _projectiles[i].SetActive(false);
            }
        }

        public KinematicProjectileLauncher Initialize(IMonsterInternal owner, PlatformManager platformManager, params string[] collisionTags)
        {
            _owner = owner;
            _platformManager = platformManager;
            _collisionTags = collisionTags;

            return this;
        }

        public KinematicProjectileLauncher SetProjectileInitializer(params Action<KinematicProjectile>[] initializers)
        {
            _projectileInitializers = initializers;
            return this;
        }


        public void LaunchWithRotation(float speed, Vector2 direction)
        {
            ThrowIfNotValidState();

            for (int i = 0; i < _projectiles.Length; i++)
            {
                if (!_projectiles[i])
                    throw new InvalidOperationException(Ctx($"인덱스 {i}에 있는 투사체 프리팹이 존재하지 않거나 유효하지 않습니다."));

                var projectile = Instantiate(_projectiles[i]);

                projectile.transform.position = _owner.transform.position + _projectilePositions[i];
                projectile.SetActive(true);

                if (!projectile.TryGetComponent<KinematicProjectile>(out var component))
                    throw new InvalidOperationException(
                        Ctx($"투사체 {projectile.name}이(가) {nameof(KinematicProjectile)} 컴포넌트를 가지고 있지 않습니다."));

                InitializeProjectile(component);
                component.transform.rotation = RotationFromDirection(direction);

                component.Launch(component.transform.right, speed);
            }
        }

        public static Quaternion RotationFromDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude <= Mathf.Epsilon) return Quaternion.identity;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void LaunchWithLocalRotation(float speed, Vector2 directionUnit)
        {
            ThrowIfNotValidState();

            for (int i = 0; i < _projectiles.Length; i++)
            {
                var projectile = Instantiate(_projectiles[i]);

                projectile.transform.position = _owner.transform.position + _projectilePositions[i];
                projectile.SetActive(true);

                var component = projectile.GetComponent<KinematicProjectile>();
                InitializeProjectile(component);
                component.Launch(projectile.transform.rotation * directionUnit, speed);
            }
        }

        public void LaunchWithDirections(float speed, params Vector2[] directions)
        {
            ThrowIfNotValidState();

            for (int i = 0; i < _projectiles.Length; i++)
            {
                var projectile = Instantiate(_projectiles[i]);

                projectile.transform.position = _owner.transform.position + _projectilePositions[i];
                projectile.SetActive(true);

                var component = projectile.GetComponent<KinematicProjectile>();
                InitializeProjectile(component);
                component.Launch(directions[i], speed);
            }
        }

        private KinematicProjectile InitializeProjectile(KinematicProjectile projectile)
        {
            projectile.Initialize(_platformManager, _collisionTags);

            if (_projectileInitializers != null)
            {
                foreach (var initializer in _projectileInitializers)
                    initializer?.Invoke(projectile);
            }

            return projectile;
        }



        private void ThrowIfNotValidState()
        {
            if (!_owner.IsValid() || !_platformManager)
                throw new InvalidOperationException(Ctx(
                    $"{name} 객체의 KinematicProjectileLauncher 컴포넌트가 유효하지 않은 상태입니다. " +
                    "컴포넌트를 사용하기 전에 Initialize()를 호출하였는지 확인하세요."));
        }

        private string Ctx(string message) => $"[KinematicProjectileLauncher of ({name})] {message}";
    }
}
