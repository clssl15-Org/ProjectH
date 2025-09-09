using UnityEngine;

public class KinematicProjectile : Projectile
{
    protected Vector3 Direction { get; set; }
    protected float Speed { get; set; }


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
