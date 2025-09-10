using UnityEngine;

public class KinematicProjectile : Projectile
{
    protected Vector3 Direction { get; set; }
    protected float Speed { get; set; }


    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();

        rb.isKinematic = true;
        rb.useFullKinematicContacts = true;
    }

    public void Launch(Vector2 direction, float speed)
    {
        Direction = direction.normalized;
        Speed = speed;
    }

    protected override void Update()
    {
        transform.position += Time.deltaTime * Speed * Direction;
        base.Update();
    }
}
