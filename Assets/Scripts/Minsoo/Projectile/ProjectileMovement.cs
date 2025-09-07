using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [SerializeField]
    private float duration = 1f;

    [SerializeField]
    private float speed = 10f;

    private float dt;
    private float projectileCursor;
    private Vector2 direction = Vector2.right;

    private void Update()
    {
        float animationDt = dt / duration;
        projectileCursor += animationDt;

        if (projectileCursor >= 1f)
        {
            Destroy(gameObject);
        }

        float moveDistance = speed * dt;
        transform.Translate(direction * moveDistance);
    }

    public void ResetProjectile(float dt, Vector2 direction)
    {
        this.dt = dt;
        this.direction = direction;
        projectileCursor = 0f;
    }
}
