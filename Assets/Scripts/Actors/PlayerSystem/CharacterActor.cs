using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Actor.PlayerSystem
{
    public class CharacterActor : MonoBehaviour
    {
        [Header("Ground Check")]
        [SerializeField]
        private float groundCheckRadius = 0.15f;

        [SerializeField]
        private float groundCheckOffset = 0.5f;

        [SerializeField]
        private LayerMask groundLayer = ~0;

        [Header("LandedCheck")]
        [SerializeField]
        private float landedTimer = 0.1f;

        Rigidbody2D _rigidbody = null;
        CapsuleCollider2D _collider = null;
        public Animator Animator { get; private set; }
        public SpriteRenderer PlayerSpriteRenderer { get; private set; }
        public Rigidbody2D Rigidbody
        {
            get => _rigidbody;
        }
        public CapsuleCollider2D Collider
        {
            get => _collider;
        }
        public float Size
        {
            get => (transform.localScale.x + transform.localScale.y) * 0.5f;
            set => transform.localScale = new Vector3(value, value, value);
        }
        public Vector2 ColliderCenter
        {
            get => (Vector2)transform.position + Vector2.Scale(_collider.offset, transform.localScale);
        }
        public Vector2 Velocity
        {
            get => _rigidbody.velocity;
            set => _rigidbody.velocity = value;
        }
        public Vector2 PlanarVelocity
        {
            get => new Vector2(Velocity.x, 0);
        }
        public Vector2 VerticalVelocity
        {
            get => new Vector2(0, Velocity.y);
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
        public Vector2 FacingDirection
        {
            get => facingDirection;
            set => facingDirection = value;
        }
        public Vector2 Forward
        {
            get => Rotation * Vector2.right * facingDirection;
        }
        public Vector2 Backward
        {
            get => Rotation * Vector2.left * facingDirection;
        }

        public bool IsGrounded { get; private set; }
        public bool PreviousIsGrounded { get; private set; }
        public bool IsLanded { get; private set; }

        private Vector2 facingDirection = Vector2.right;
        private bool isGroundedFlag = false;
        private float landedCursor = 0f;

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

        void UpdateLandingState(float dt)
        {
            // Maintain the landing state as true for a short duration after
            // touching the ground to ensure accurate detection and prevent flickering.

            float landedDt = dt / landedTimer;
            landedCursor += landedDt;

            if (landedCursor >= 1f)
                IsLanded = false;

            // Detect landing moment precisely by confirming vertical velocity is zero
            // while grounded, then reset flags and timers to track landing duration.

            if (Velocity.y != 0f && !IsGrounded)
                isGroundedFlag = true;

            if (!isGroundedFlag)
                return;

            IsLanded = IsGrounded && (Velocity.y == 0f);

            if (IsLanded)
            {
                isGroundedFlag = false;
                landedCursor = 0f;
            }
        }

        void ProbeGround(float dt)
        {
            PreviousIsGrounded = IsGrounded;

            Vector2 circleOrigin = Position + Vector2.down * ((_collider.size.y * 0.5f) - (_collider.size.x * 0.5f) - groundCheckOffset);

            IsGrounded = Physics2D.OverlapCircle(
                circleOrigin,
                groundCheckRadius,
                groundLayer
            ) != null;
        }

        public void ChangeFlipX(Vector2 inputValue)
        {
            if (inputValue.x == 0f)
                return;

            if (inputValue.x > 0f)
            {
                PlayerSpriteRenderer.flipX = false;
                FacingDirection = Vector2.right;
            }

            else
            {
                PlayerSpriteRenderer.flipX = true;
                FacingDirection = Vector2.left;
            }
        }

#if UNITY_EDITOR
        // Draws a wire sphere in the editor to visualize the ground check area
        private void OnDrawGizmosSelected()
        {
            if (!TryGetComponent(out CapsuleCollider2D c)) return;

            Vector2 circleOrigin = (Vector2)transform.position + Vector2.down *
                ((c.size.y * 0.5f) - (c.size.x * 0.5f) - groundCheckOffset);

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

            PlayerSpriteRenderer = this.transform.root.GetComponentInChildren<SpriteRenderer>();
        }
        private void FixedUpdate()
        {
            float dt = Time.deltaTime;

            ProbeGround(dt);
            UpdateLandingState(dt);
            //PreSimulationUpdate(dt);

            //transform.SetPositionAndRotation(Position, Rotation);
        }
    }
}
