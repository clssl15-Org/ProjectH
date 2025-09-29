using UnityEngine;

public class KnockbackHandler
{
    // Front
    public float KnockbackForce { get; set; } = 150f;

    // Internal
    Rigidbody2D _rigidbody;


    // Content
    public KnockbackHandler(Rigidbody2D rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public void Knockback(Direction direction, float? knockbackForce = null)
    {
        if (!_rigidbody)
            throw new System.InvalidOperationException($"{nameof(_rigidbody)} 컴포넌트가 유효한 상태가 아닙니다.");

        _rigidbody.AddForce(direction.ToVector2() * (knockbackForce ?? KnockbackForce));
    }
}
