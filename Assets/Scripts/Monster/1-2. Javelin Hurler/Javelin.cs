using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Javelin : MonoBehaviour
{
    public PlatformManager platformManager;

    public void Throw(Quaternion direction, float power)
        => Throw(direction * Vector2.right, power);



    public void Throw(Vector2 direction, float power)
    {
        GetComponent<Rigidbody2D>().velocity = power * direction.normalized;
    }

    private void Update()
    {
        if (!platformManager)
        {
            Debug.LogWarning("platformManager가 없으므로 Javelin을 삭제합니다.");
            Destroy();
            return;
        }

        if (!platformManager.Bound.Contains(transform.position))
            Destroy();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            Destroy();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
