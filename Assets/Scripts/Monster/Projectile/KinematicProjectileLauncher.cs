using System;
using UnityEngine;

public class KinematicProjectileLauncher : MonoBehaviour
{
    // Front
    public enum LaunchType
    {
        Rotation,
        LocalRotation,
        Directions
    }

    // Property
    [SerializeField] private GameObject[] projectiles;

    // Internal
    private Vector3[] projectilesPositions;

    private Monster owner;
    private PlatformManager platformManager;
    private string[] collisionTags;


    // Content
    private void Start()
    {
        projectilesPositions = new Vector3[projectiles.Length];

        for (int i = 0; i < projectiles.Length; i++)
        {
            projectilesPositions[i] = projectiles[i].transform.localPosition;
            projectiles[i].SetActive(false);
        }
    }

    public virtual void Initialize(Monster owner, PlatformManager platformManager, params string[] collisionTags)
    {
        this.owner = owner;
        this.platformManager = platformManager;
        this.collisionTags = collisionTags;
    }


    public void LaunchWithRotation(float speed, Vector2 direction)
    {
        ThrowIfNotValidState();

        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i])
                throw new InvalidOperationException(Ctx($"인덱스 {i}에 있는 투사체 프리팹이 존재하지 않거나 유효하지 않습니다."));

            var projectile = Instantiate(projectiles[i]);

            projectile.transform.position = owner.transform.position + projectilesPositions[i];
            projectile.SetActive(true);

            if (!projectile.TryGetComponent<KinematicProjectile>(out var component))
                throw new InvalidOperationException(
                    Ctx($"투사체 {projectile.name}이(가) {nameof(KinematicProjectile)} 컴포넌트를 가지고 있지 않습니다."));

            component.Initialize(platformManager, collisionTags);
            component.transform.rotation = RotationFromDirection(direction);

            component.Launch(component.transform.right, speed);
        }

        Quaternion RotationFromDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude <= Mathf.Epsilon) return Quaternion.identity;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void LaunchWithLocalRotation(float speed, Vector2 directionUnit)
    {
        ThrowIfNotValidState();

        for (int i = 0; i < projectiles.Length; i++)
        {
            var projectile = Instantiate(projectiles[i]);

            projectile.transform.position = owner.transform.position + projectilesPositions[i];
            projectile.SetActive(true);

            var component = projectile.GetComponent<KinematicProjectile>();
            component.Initialize(platformManager, collisionTags);
            component.Launch(projectile.transform.rotation * directionUnit, speed);
        }
    }

    public void LaunchWithDirections(float speed, params Vector2[] directions)
    {
        ThrowIfNotValidState();

        for (int i = 0; i < projectiles.Length; i++)
        {
            var projectile = Instantiate(projectiles[i]);

            projectile.transform.position = owner.transform.position + projectilesPositions[i];
            projectile.SetActive(true);

            var component = projectile.GetComponent<KinematicProjectile>();
            component.Initialize(platformManager, collisionTags);
            component.Launch(directions[i], speed);
        }
    }

    
    private void ThrowIfNotValidState()
    {
        if (!owner || !platformManager)
            throw new InvalidOperationException(Ctx(
                $"{name} 객체의 KinematicProjectileLauncher 컴포넌트가 유효하지 않은 상태입니다. " +
                "컴포넌트를 사용하기 전에 Initialize()를 호출하였는지 확인하세요."));
    }

    private string Ctx(string message) => $"[KinematicProjectileLauncher] {message}";
}
