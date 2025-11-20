using System;
using UnityEngine;

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
        private Vector3[] _projectilesPositions;

        private IMonsterInternal _owner;
        private PlatformManager _platformManager;
        private string[] _collisionTags;


        // Content
        private void Start()
        {
            _projectilesPositions = new Vector3[_projectiles.Length];

            for (int i = 0; i < _projectiles.Length; i++)
            {
                _projectilesPositions[i] = _projectiles[i].transform.localPosition;
                _projectiles[i].SetActive(false);
            }
        }

        public void Initialize(IMonsterInternal owner, PlatformManager platformManager, params string[] collisionTags)
        {
            _owner = owner;
            _platformManager = platformManager;
            _collisionTags = collisionTags;
        }


        public void LaunchWithRotation(float speed, Vector2 direction)
        {
            ThrowIfNotValidState();

            for (int i = 0; i < _projectiles.Length; i++)
            {
                if (!_projectiles[i])
                    throw new InvalidOperationException(Ctx($"인덱스 {i}에 있는 투사체 프리팹이 존재하지 않거나 유효하지 않습니다."));

                var projectile = Instantiate(_projectiles[i]);

                projectile.transform.position = _owner.transform.position + _projectilesPositions[i];
                projectile.SetActive(true);

                if (!projectile.TryGetComponent<KinematicProjectile>(out var component))
                    throw new InvalidOperationException(
                        Ctx($"투사체 {projectile.name}이(가) {nameof(KinematicProjectile)} 컴포넌트를 가지고 있지 않습니다."));

                component.Initialize(_platformManager, _collisionTags);
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

                projectile.transform.position = _owner.transform.position + _projectilesPositions[i];
                projectile.SetActive(true);

                var component = projectile.GetComponent<KinematicProjectile>();
                component.Initialize(_platformManager, _collisionTags);
                component.Launch(projectile.transform.rotation * directionUnit, speed);
            }
        }

        public void LaunchWithDirections(float speed, params Vector2[] directions)
        {
            ThrowIfNotValidState();

            for (int i = 0; i < _projectiles.Length; i++)
            {
                var projectile = Instantiate(_projectiles[i]);

                projectile.transform.position = _owner.transform.position + _projectilesPositions[i];
                projectile.SetActive(true);

                var component = projectile.GetComponent<KinematicProjectile>();
                component.Initialize(_platformManager, _collisionTags);
                component.Launch(directions[i], speed);
            }
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
