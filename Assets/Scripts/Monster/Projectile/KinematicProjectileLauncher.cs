using UnityEngine;

public class KinematicProjectileLauncher : MonoBehaviour
{
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
        for (int i = 0; i < projectiles.Length; i++)
        {
            var projectile = Instantiate(projectiles[i]);

            projectile.transform.position = owner.transform.position + projectilesPositions[i];
            projectile.SetActive(true);

            var component = projectile.GetComponent<KinematicProjectile>();
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
}
