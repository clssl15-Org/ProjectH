using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActor : MonoBehaviour
{
    Rigidbody2D _rigidbody = null;
    CapsuleCollider2D _collider = null;
    public Vector2 Velocity
    {
        get => _rigidbody.velocity;
        set => _rigidbody.velocity = value;
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

        Velocity = (position - Position) / dt;
    }

    void ProcessStableMovement()
    {

    }

    void ProcessUnstableMovement()
    {

    }

    private void Awake()
    {
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

        PreSimulationUpdate(dt);
        
        //이 함수로 인해 위치 값이 변경됨. position을 바꿔주는 기능과 input값을 받아들이는 함수는 어디?
        transform.SetPositionAndRotation(Position, Rotation);
        //오히려 physicsActor의 PlanarVelocity가 바꿔주는것 같기도
        // 일단, CharacterStateController - FixedUpdate - movementReferenceParameters에서 @movement값 전달
        // forward * @movement값이 movementReferenceParameters의 InputMovementReference 변수에 저장되는데
        // 결국, CharacterStateController가 프로퍼티로 가지고 있으며, 외부에서 이것을 참조함.
        // zeroGravity의 ProcessVelocity()함수 에서 이 값을 사용하여 CharacterActor.Velocity값을 바꿔줌.
        // zeroGravity는 CharacterState를 상속받으며, UpdateBehaviour에서 ProcessVelocity()함수를 매프레임 실행함.
    }
}
