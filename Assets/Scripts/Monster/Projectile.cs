using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    private PlatformManager platformManager;
    private string[] collisionTags;


    public T Initialize<T>(PlatformManager platformManager, params string[] collisionTags) where T : Projectile
    {
        Initialize(platformManager, collisionTags);
        return (T)this;
    }
    public virtual void Initialize(PlatformManager platformManager, params string[] collisionTags) 
    {
        this.platformManager = platformManager;
        this.collisionTags = collisionTags;
    }

    protected virtual void Update()
    {
        if (!platformManager)
        {
            Debug.LogError($"PlatformManager가 없기 때문에 Projectile({name})을 사용할 수 없습니다.");
            Destroy();
            return;
        }

        if (!platformManager.Bound.Contains(transform.position))
            Destroy();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionTags == null || collisionTags.Length == 0)
            return;

        if (collisionTags.Any(t => collision.gameObject.CompareTag(t)))
            Destroy();
    }

    public virtual void Destroy()
    {
        Destroy(gameObject);
    }
}
