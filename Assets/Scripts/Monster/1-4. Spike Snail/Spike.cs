using UnityEngine;

public class Spike : Projectile
{
    private Vector3 direction;
    private float speed;


    public void Launch(Vector2 direction, float speed)
    {
        this.direction = direction.normalized;
        this.speed = speed;
    }

    protected override void Update()
    {
        transform.position += Time.deltaTime * speed * direction;
        base.Update();
    }
}
