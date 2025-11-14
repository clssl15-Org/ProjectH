using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D playerRigidbody;
    private PlayerState playerState;
    [SerializeField] private SpriteRenderer playerBodySpriteRenderer;   // 플레이어스프라이트의 spriteRenderer
    private Collider2D playerCollider;

    [Header("Movement Settings")] // 이동 관련 설정들을 묶어줍니다.
    [SerializeField] private float moveSpeed = 5f;  // 이동 속도도
    [SerializeField] private float jumpPower = 7f;  // 점프 파워
    [SerializeField] private float doubleJumpPower = 7f;    // 더블 점프 파워
    [SerializeField] private bool doubleJump = true;   // 더블 점프 가능?

    [Header("Gravity Settings")] // 플레이어 중력 설정
    [SerializeField] private float normalGravityScale = 1f;  // 기본 중력
    [SerializeField] private float fallingGravityScaleMultiplier = 1.5f;  // 낙하 시 중력 배율

    [Header("Dash Settings")] // 대쉬 관련 설정들을 묶어줍니다.
    [SerializeField] private float dashSpeed = 15f;     // 대쉬 속도도
    [SerializeField] private float dashDistance = 3f;   // 대쉬 거리
    [SerializeField] private float dashCooldown = 1f; // 대쉬 쿨타임
    [SerializeField] private float dashIFrame = 0.5f;     // 대쉬 무적 지속 시간간


    // 상태 관련 변수
    private bool canDoubleJump = true; // 더블 점프를 뛸 수 있는 상태인지지
    private bool canDash = true; // 대쉬 가능 여부를 나타내는 변수


    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerState = PlayerState.Instance;

        // 자식 오브젝트 (PlayerBody)의 컴포넌트 가져오기 
        playerCollider = transform.Find("PlayerBody").gameObject.GetComponent<Collider2D>();
    }

    void Update()
    {
        if (playerState.currentState != PlayerState.State.Hitted && !playerState.isDead)
        {    // 경직 상태가 아니라면면
            Move();
            Jump();
            Dash();
        }
    }

    void Move() // 무빙
    {
        if (playerState.IsBasicState() || playerState.currentState == PlayerState.State.AirAttack)  // 플레이어가 idle, running, jumping, falling, AirAttack 상태일때만 작동
        {
            // 이동
            float moveInput = Input.GetAxis("Horizontal"); // A/D로 이동
            playerRigidbody.velocity = new Vector2(moveInput * moveSpeed, playerRigidbody.velocity.y);
            playerBodySpriteRenderer.flipX = moveInput > 0 ? false : moveInput < 0 ? true : playerBodySpriteRenderer.flipX;  // 플레이어가 움직인 방향에 따라 스프라이트 뒤집기
        }
    }

    void Jump() // 점프
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && playerState.IsBasicState()) // 스페이스바로 점프 and 기본 상태(idle, running, jumping, falling) 일경우만 작동
        {
            if (playerState.isGrounded)
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, jumpPower);
                playerState.isGrounded = false;
            }
            else if (canDoubleJump && doubleJump)
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, doubleJumpPower);
                canDoubleJump = false;
            }
        }
    }

    void Dash() // 대쉬
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && playerState.currentState != PlayerState.State.Dashing // Shift키로 대쉬
        && playerState.currentState != PlayerState.State.QSkill) // 궁극기는 캔슬 불가
        {
            StartCoroutine(DashCoroutine());
        }
    }

    IEnumerator DashCoroutine()
    {
        if (canDash)
        {
            StartCoroutine(dashIFrameCoroutine());  // 무적 시작

            float startPosition = transform.position.x; // 대쉬 시작 위치 저장
            canDash = false; // 대쉬 불가능 상태로 변경
            playerState.ChangeState(PlayerState.State.Dashing); // 대쉬 상태로 변경

            float dashDirection; // 대쉬 방향
            if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f)
            {    // 누르고 있는 버튼이 없다면.
                dashDirection = playerState.isFacingRight ? -1f : 1f; // 바라보는 방향에 따라 대쉬 방향 결정
            }
            else
            {
                dashDirection = Mathf.Sign(Input.GetAxis("Horizontal"));    // 누르고 있는 방향으로 대쉬 방향 결정
            }


            // 처음 대쉬 (아무것도 안 누른 상태에서도 대쉬 되게)
            while (Mathf.Abs(transform.position.x - startPosition) < 0.01f)
            {
                playerRigidbody.velocity = new Vector2(dashDirection * dashSpeed, playerRigidbody.velocity.y);  // 대쉬
                yield return null; // 프레임별로 업데이트

                if (Mathf.Abs(playerRigidbody.velocity.x) < 0.01f) break; // 벽에 부딪혀서 속도가 0에 가까워졌을때 대쉬가 멈추게
            }

            // 대쉬 실행        
            while (Mathf.Abs(transform.position.x - startPosition) < dashDistance) // 거리를 설정한 만큼 가거나
            {
                playerRigidbody.velocity = new Vector2(dashDirection * dashSpeed, playerRigidbody.velocity.y);  // 대쉬
                yield return null; // 프레임별로 업데이트

                if (Mathf.Abs(playerRigidbody.velocity.x) < 0.01f) break; // 벽에 부딪혀서 속도가 0에 가까워졌을때 대쉬가 멈추게
            }

            playerState.ReturnToBasicState(); // 대쉬 상태 종료 후 기본 상태로 변경

            // 쿨다운 타이머
            yield return new WaitForSeconds(dashCooldown); // 설정한 시간 동안 대기
            canDash = true; // 대쉬 가능 상태로 복구
        }
    }

    IEnumerator dashIFrameCoroutine()
    { // 대쉬 무적 코루틴
        playerState.isInvincible = true; // 무적 상태
        yield return new WaitForSeconds(dashIFrame); // 설정한 시간 동안 대기
        playerState.isInvincible = false; // 무적x 상태
    }


    void FixedUpdate()
    {

        // 점프 초기화
        if (playerRigidbody.velocity.y < 0)
        {
            Debug.DrawRay(playerRigidbody.position, Vector3.down, new Color(0, 1, 0));

            // Ground tag에 닿는지 확인 (플레이어 크기의 0.6을 아래로 레이히트)
            RaycastHit2D rayHit = Physics2D.Raycast(playerRigidbody.position, Vector3.down, playerCollider.bounds.size.y * 0.6f, LayerMask.GetMask("Ground"));

            if (rayHit.collider != null)
            {
                playerState.isGrounded = true;
                canDoubleJump = true;
            }
        }

        Gravity();
    }


    // 중력
    void Gravity()
    {    // 중력
        if (!playerState.isGrounded && playerRigidbody.velocity.y < 0)  // 땅에 닿아 있는 상태가 아니고, 아래로 내려가는 상태일 때.
        {
            playerRigidbody.gravityScale = normalGravityScale * fallingGravityScaleMultiplier;
        }
        else
        {
            playerRigidbody.gravityScale = normalGravityScale;
        }
    }
}
