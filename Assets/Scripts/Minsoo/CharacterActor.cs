using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActor : MonoBehaviour
{
    [Header("Ground Check")]
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer = ~0;

    Rigidbody2D _rigidbody = null;
    CapsuleCollider2D _collider = null;
    public Animator Animator { get; private set; }
    public Vector2 Velocity
    {
        get => _rigidbody.velocity;
        set => _rigidbody.velocity = value;
    }
    public Vector2 PlanarVelocity
    {
        get => new Vector2(Velocity.x, 0);
    }

    public Vector2 Position
    {
        get => new Vector2(_rigidbody.position.x, _rigidbody.position.y);
        set => _rigidbody.position = value;
    }
    public Quaternion Rotation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }

    public bool IsGrounded { get; private set; }

    private void PreSimulationUpdate(float dt)
    {
        ProcessVelocity(dt);
    }

    void ProcessVelocity(float dt)
    {
        Vector2 position = Position;

        if (IsGrounded)
            ProcessStableMovement();
        else
            ProcessUnstableMovement();
        
        //Velocity = (position - Position) / dt;
        //Debug.Log("Velocity: " + Velocity + " Position: " + Position);
    }

    void ProcessStableMovement()
    {

    }

    void ProcessUnstableMovement()
    {

    }

    void ProbeGround(float dt)
    {
        Vector2 circleOrigin = Position + Vector2.down * ((_collider.size.y * 0.5f) - (_collider.size.x * 0.5f));

        IsGrounded = Physics2D.OverlapCircle(
            circleOrigin,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

#if UNITY_EDITOR
    // Draws a wire sphere in the editor to visualize the ground check area
    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent(out CapsuleCollider2D c)) return;

        Vector2 circleOrigin = (Vector2)transform.position + Vector2.down *
            ((c.size.y * 0.5f) - (c.size.x * 0.5f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(circleOrigin, groundCheckRadius);
    }
#endif

    private void Awake()
    {
        Animator = this.transform.root.GetComponentInChildren<Animator>();

        if (!gameObject.TryGetComponent(out Rigidbody2D rigid))
            rigid = gameObject.AddComponent<Rigidbody2D>();

        _rigidbody = rigid;

        if (!gameObject.TryGetComponent(out CapsuleCollider2D col))
            col = gameObject.AddComponent<CapsuleCollider2D>();

        _collider = col;
    }
    private void FixedUpdate()
    {
        float dt = Time.deltaTime;

        ProbeGround(dt);
        //PreSimulationUpdate(dt);

        //transform.SetPositionAndRotation(Position, Rotation);
    }
}
