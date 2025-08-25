using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Javelin : Projectile<Javelin>
{
    public void Throw(Quaternion direction, float power) => Throw(direction * Vector2.right, power);
    public void Throw(Vector2 direction, float power)
    {
        GetComponent<Rigidbody2D>().velocity = power * direction.normalized;
    }
}
