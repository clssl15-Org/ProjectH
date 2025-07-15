
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    private static PlayerState _instance = null;
    [SerializeField] private Rigidbody2D playerRigidbody;       // 플레이어의 Rigidbody
    [SerializeField] private SpriteRenderer playerBodySpriteRenderer;   // 플레이어바디의 spriteRenderer
    private Transform playerTransform;

    public static PlayerState Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerState>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(PlayerState).Name);
                    _instance = singletonObject.AddComponent<PlayerState>();
                    // DontDestroyOnLoad(singletonObject); // 씬이 바뀌어도 유지되도록 설정 (선택 사항)
                }
            }
            return _instance;
        }
    }

    // 플레이어의 가능한 모든 상태를 정의하는 enum // 애니메이션 편하게 하기 위해
    public enum State
    {
        Idle = 0,
        Running = 1,
        Jumping = 2,
        Falling = 3,
        Dashing = 4,
        Attack1 = 5,
        Attack2 = 6,
        Attack3 = 7,
        AirAttack = 8,
        ESkill = 9,
        RangedAttack = 10,
        QSkill = 11,
        Hitted = 12,
        Dead = 13,
    }

    public State currentState; // 현재 플레이어의 상태

    public bool isInvincible;   // 무적 상태
    public bool isGrounded;     // 땅에 있는지 여부
    public bool isFacingRight; // 앞을 보고 있는지 (true: 오른쪽, false: 왼쪽)
    public bool isDead; // 죽은 상태
    public bool isSkillActive; // 스킬이 활성화되어 있는지

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지되도록 설정 (선택 사항)

        // 초기 상태를 Idle로 설정
        currentState = State.Idle;
        isInvincible = false; // 초기 무적 상태는 false
        isGrounded = false;   // 초기 땅에 있는지 여부는 false
        playerRigidbody = GetComponent<Rigidbody2D>(); // 플레이어의 리지드바디 컴포넌트를 가져옴
        playerTransform = GetComponent<Transform>();    // 플레이어의 transform 컴포넌트를 가져옴

        // 초기 방향 설정
        UpdateFacingDirection();
    }

    // 상태를 변경하는 메서드
    public void ChangeState(State newState)
    {
        // 스킬이 활성화되어 있고, 현재 상태가 QSkill이면서 새로운 상태가 Hitted나 Dead가 아닌 경우 상태 변경을 방지
        if (isSkillActive && currentState == State.QSkill && 
            newState != State.Hitted && newState != State.Dead)
        {
            return;
        }


        if (currentState == newState)
            return;

        currentState = newState;

        // 상태 변경에 따른 추가적인 로직 (예: 애니메이션 트리거)을 여기에 구현할 수 있습니다.
        // Debug.Log("Player State Changed to: " + currentState);
    }


    // 매 프레임마다 플레이어의 기본 상태를 업데이트하는 메서드 
    void UpdateBasicState()
    {
        if (currentState == State.Jumping || currentState == State.Falling || currentState == State.Idle || currentState == State.Running)
        {
            if (playerRigidbody.velocity.y > 0.1f && !isGrounded)   //  점프 상태태
            {
                ChangeState(State.Jumping);
            }
            else if (playerRigidbody.velocity.y < -0.1f && !isGrounded)     // 낙하 상태
            {
                ChangeState(State.Falling);
            }
            else if (isGrounded && Mathf.Abs(playerRigidbody.velocity.x) < 0.1f && currentState != State.Idle) // 아이들 상태
            {
                ChangeState(State.Idle);
            }
            else if (isGrounded && Mathf.Abs(playerRigidbody.velocity.x) >= 0.1f && currentState != State.Running) // 런닝 상태
            {
                ChangeState(State.Running);
            }
        }
    }

    // 스킬, 대쉬 사용후 기본 상태로 업데이트
    public void ReturnToBasicState()
    {
        if (isGrounded)     // 땅에 있으면 idle 상태로
        {
            ChangeState(State.Idle);
        }
        else    // 땅에 있지 않으면 점프 상태 또는 낙하 상태로
        {
            if (playerRigidbody.velocity.y > 0.1f)
            {
                ChangeState(State.Jumping);
            }
            else if (playerRigidbody.velocity.y < -0.1f)
            {
                ChangeState(State.Falling);
            }
            else
            {
                ChangeState(State.Idle); // 공중에서 멈췄을 경우 (드물지만)
            }
        }
    }

    void Update()
    {
        UpdateBasicState();
        UpdateFacingDirection();
        CheckPlayerPosition();
        if (isDead)
        {
            ChangeState(State.Dead);
        }
    }



    // 상태 점검 메서드, 기본 상태인지 확인
    // 스킬 or 공격 가능한 상태인지 반환
    // 기본 상태 : Idle, Running, Jumping, Falling
    public bool IsBasicState()
    {
        return currentState == State.Idle || currentState == State.Running || currentState == State.Jumping || currentState == State.Falling;
    }


    // PlayerBody의 스프라이트 flipX 값을 기반으로 isFacingRight 업데이트
    private void UpdateFacingDirection()
    {
        if (playerBodySpriteRenderer != null)
        {
            isFacingRight = playerBodySpriteRenderer.flipX; // flipX가 true면 왼쪽, false면 오른쪽을 보고 있음
        }
    }


    // -20 보다 떨어지면 처음으로
    private void CheckPlayerPosition()
    {
        if (playerTransform.position.y < -20)
        {
            playerTransform.position = new Vector3(0, 0, 0);
        }
    }


}
