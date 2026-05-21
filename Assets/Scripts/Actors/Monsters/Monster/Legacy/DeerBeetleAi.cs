using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerBeatleAI : MonsterBase
{
    Rigidbody2D rigidbody2D;
    [SerializeField] GameObject MonsterAttack;

    private float moveSpeed = 4f; // 이동 속도
    private float atk = 20f;
    private float minActionInterval = 1f; // 행동 변경 최소 주기
    private float maxActionInterval = 3f; // 행동 변경 최대 주기
    private float cliffDepth = 1.5f; // 낭떨어지 감지 깊이
    private float detectRange = 5f;
    private float attackRange = 2f;
    private float RattackRange = 4f;

    // public float maxHP = 3;
    // private float currentHP;
    public float cliffDetectDistance = 2f; // 또는 2.0f 등
    public LayerMask groundLayer; // 바닥 레이어
    public float attackCooldown = 1.0f; // 공격 쿨타임(초)
    private bool isAttackCooldown = false;

    // public enum State { Idle, Detect, Run, Attack, Hit, Dead }
    // public State currentState = State.Run;
    private bool isOnGround = false; // 몬스터가 Ground 테그에 닿아있는 지 확인
    private Transform playerTransform; // 플레이어 Transform 저장
    private bool isPlayerInRange = false; // 플레이어가 범위 내에 있는지
    private bool isCliffAhead = false; // 낭떠러지 감지 결과 저장
    private bool canChaseLeft = true;
    private bool canChaseRight = true;
    private bool movingRight = true; // true면 오른쪽, false면 왼쪽
    private int idleMoveDirection = 1; // Idle 상태에서 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private float idleMoveTime = 0f;
    public float idleMoveDuration = 2f; // 한 방향으로 이동하는 시간
    private bool isAttacking = false;
    private bool isIdleDelay = false;

    void Start()
    {
        maxHP = 100f;
        currentHP = maxHP;
        MonsterAttack.SetActive(false);

        rigidbody2D = GetComponent<Rigidbody2D>();
        StartCoroutine(StateRoutine());
    }

    void Update()
    {
        // Debug.Log(currentState);
        if (currentState == State.Idle)
        {
            idleMoveTime += Time.deltaTime;
            if (idleMoveTime >= idleMoveDuration)
            {
                idleMoveDirection *= -1; // 방향 전환
                idleMoveTime = 0f;
            }
        }
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Detect:
                FacePlayer();
                Detect();
                break;
            case State.Run:
                Run();
                break;
            case State.Attack:
                Attack();
                break;
            case State.LAttack:
                Attack();
                break;
            case State.RAttack:
                Attack();
                break;
            case State.Hit:
                Hit();
                break;
            case State.Dead:
                Dead();
                break;
        }
    }

    IEnumerator StateRoutine()
    {
        while (currentState != State.Dead)
        {
            yield return new WaitForSeconds(0.2f);

            bool inTrigger = PlayerDetected();
            bool inAttackRange = PlayerInAttackRange();

            // Detect 상태에서만 Idle로 진입
            if (currentState == State.Detect)
            {
                if (inAttackRange && !isAttackCooldown)
                    ChangeState(State.Idle);
            }
            else if (currentState == State.Run)
            {
                if (inTrigger)
                    ChangeState(State.Detect);
            }
            else if (currentState == State.Hit)
            {
                if (currentHP <= 0)
                    ChangeState(State.Dead);
                else
                    ChangeState(State.Idle);
            }
            // Idle 상태에서는 IdleDelayCoroutine에서만 상태 전환
            // Attack 상태에서는 AttackCoroutine에서만 상태 전환
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        if (newState == State.Idle && !isIdleDelay)
        {
            StartCoroutine(IdleDelayCoroutine());
        }
    }

    void Idle()
    {
        rigidbody2D.velocity = new Vector2(0, 0);
        // 아무것도 하지 않음 (정지 상태)
    }

    void Detect()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 플레이어 추적 로직
        float direction = playerTransform.position.x - transform.position.x;
        // 낭떠러지 체크 (Run과 동일하게)
        float offsetX = Mathf.Sign(direction) * 0.5f;
        float offsetY = 0f;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            offsetY = -col.bounds.extents.y;
        Vector2 frontPos = (Vector2)transform.position + new Vector2(offsetX, offsetY);

        bool groundAhead = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(frontPos, 0.05f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Ground"))
            {
                groundAhead = true;
                break;
            }
        }
        if (!groundAhead)
        {
            rigidbody2D.velocity = Vector2.zero;
            idleMoveTime = 0f;
            return;
        }
        // 플레이어 방향으로 이동
        rigidbody2D.velocity = new Vector2(Mathf.Sign(direction) * moveSpeed, rigidbody2D.velocity.y);
    }

    void Run()
    {
        // 평상시 이동(순찰) 로직
        idleMoveTime += Time.deltaTime;
        if (idleMoveTime >= idleMoveDuration)
        {
            idleMoveDirection *= -1; // 방향 전환
            idleMoveTime = 0f;
        }

        float offsetX = idleMoveDirection * 0.5f;
        float offsetY = 0f;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            offsetY = -col.bounds.extents.y;
        Vector2 frontPos = (Vector2)transform.position + new Vector2(offsetX, offsetY);
        Debug.DrawLine(transform.position, frontPos, Color.green);
        Debug.DrawLine(frontPos, frontPos + Vector2.down * 0.3f, Color.blue);
        bool groundAhead = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(frontPos, 0.05f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Ground"))
            {
                groundAhead = true;
                break;
            }
        }
        if (!groundAhead)
        {
            rigidbody2D.velocity = Vector2.zero;
            idleMoveDirection *= -1; // 낭떠러지면 방향 전환
            idleMoveTime = 0f;
            return;
        }
        rigidbody2D.velocity = new Vector2(idleMoveDirection * moveSpeed, rigidbody2D.velocity.y);
    }

    void Attack()
    {
        FacePlayer();
        if (PlayerDetected())
        {
            if (!isAttacking)
            {
                MonsterAttack.SetActive(true);
                int attackPattern = UnityEngine.Random.Range(0, 10);
                if (attackPattern >= 9)
                {
                    ChangeState(State.LAttack);
                    StartCoroutine(LAttackCoroutine());
                }
                else
                {
                    if (Vector2.Distance(transform.position, playerTransform.position) <= RattackRange - 1f)
                    {
                        ChangeState(State.Attack);
                        StartCoroutine(AttackCoroutine());
                    }
                    else
                    {
                        ChangeState(State.RAttack);
                        StartCoroutine(RAttackCoroutine());
                    }
                }
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        // 공격 실행
        Transform attackObj = transform.GetChild(0);
        if (attackObj != null)
        {
            var attackScript = attackObj.GetComponent<MonsterAttack>();
            if (attackScript != null)
            {
                attackScript.DeerAttack();
            }
        }
        MonsterAttack.SetActive(false);
        StartCoroutine(AttackCooldownCoroutine());

        // 공격 후 다음 상태 판정
        bool inTrigger = PlayerDetected();
        bool inAttackRange = PlayerInAttackRange();

        if (inTrigger && inAttackRange)
            ChangeState(State.Idle);    // 다시 Idle(공격 대기)
        else if (inTrigger)
            ChangeState(State.Detect);  // 추적

        MonsterAttack.SetActive(false);
        isAttacking = false;
    }
    IEnumerator LAttackCoroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        // 공격 실행
        Transform attackObj = transform.GetChild(0);
        if (attackObj != null)
        {
            var attackScript = attackObj.GetComponent<MonsterAttack>();
            if (attackScript != null)
            {
                attackScript.DeerLAttack();
            }
        }

        StartCoroutine(AttackCooldownCoroutine());

        // 공격 후 다음 상태 판정
        moveSpeed = 3f;
        bool inTrigger = PlayerDetected();
        bool inAttackRange = PlayerInAttackRange();

        if (inTrigger && inAttackRange)
            ChangeState(State.Idle);    // 다시 Idle(공격 대기)
        else if (inTrigger)
            ChangeState(State.Detect);  // 추적
        MonsterAttack.SetActive(false);
        isAttacking = false;
    }
    IEnumerator RAttackCoroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        // 공격 실행
        Transform attackObj = transform.GetChild(0);
        if (attackObj != null)
        {
            var attackScript = attackObj.GetComponent<MonsterAttack>();
            if (attackScript != null)
            {
                attackScript.DeerRAttack();
            }
        }

        StartCoroutine(AttackCooldownCoroutine());

        // 공격 후 다음 상태 판정
        bool inTrigger = PlayerDetected();
        bool inAttackRange = PlayerInAttackRange();

        if (inTrigger && inAttackRange)
            ChangeState(State.Idle);    // 다시 Idle(공격 대기)
        else if (inTrigger)
            ChangeState(State.Detect);  // 추적

        MonsterAttack.SetActive(false);
        isAttacking = false;
    }

    void Hit()
    {
        // 피격 애니메이션/로직
        // currentHP는 외부에서 감소시켜야 함(예: 플레이어 공격 시)
        if (currentHP <= 0)
            ChangeState(State.Dead);
        else if (((currentHP / maxHP) * 100) <= 50)
        {
            attackCooldown = attackCooldown / 2;
        }
        else
            ChangeState(State.Run);
    }

    void Dead()
    {
        NotifyMonsterDied();

        rigidbody2D.velocity = Vector2.zero;
        Destroy(gameObject, 1.5f);
    }

    bool PlayerDetected()
    {
        return isPlayerInRange;
    }

    bool PlayerInAttackRange()
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= RattackRange;
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;
        currentHP -= damage;
        ChangeState(State.Hit);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // 몬스터 몸체가 Ground 태그와 닿아 있는지 확인
        if (collision.collider.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Ground와의 충돌이 끝났을 때
        if (collision.collider.CompareTag("Ground"))
        {
            isOnGround = false;
        }
    }

    // Trigger Collider에 플레이어가 들어왔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState != State.Attack && currentState != State.LAttack && currentState != State.RAttack)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
            ChangeState(State.Detect);
        }
    }

    // Trigger Collider에서 플레이어가 나갔을 때
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState != State.Attack && currentState != State.LAttack && currentState != State.RAttack)
        {
            isPlayerInRange = false;
            playerTransform = null;
            ChangeState(State.Run);
        }
    }

    void FacePlayer()
    {
        if (playerTransform == null) return;
        Vector3 scale = transform.localScale;
        if (playerTransform.position.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x); // 오른쪽 바라봄
        else
            scale.x = -Mathf.Abs(scale.x); // 왼쪽 바라봄
        transform.localScale = scale;
    }

    IEnumerator IdleDelayCoroutine()
    {
        isIdleDelay = true;
        float elapsed = 0f;
        float waitTime = 0.5f;
        while (elapsed < waitTime)
        {
            // Idle 상태가 아니면 즉시 종료
            if (currentState != State.Idle)
            {
                isIdleDelay = false;
                yield break;
            }
            // 플레이어가 트리거 밖이면 Run, 공격 사거리 밖이면 Detect
            if (!PlayerDetected() && currentState != State.Attack && currentState != State.LAttack && currentState != State.RAttack)
            {
                ChangeState(State.Run);
                isIdleDelay = false;
                yield break;
            }
            if (!PlayerInAttackRange() && currentState != State.Attack && currentState != State.LAttack && currentState != State.RAttack)
            {
                ChangeState(State.Detect);
                isIdleDelay = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 대기 끝나면 Attack으로 전환
        if (currentState == State.Idle)
            ChangeState(State.Attack);
        isIdleDelay = false;
    }

    IEnumerator AttackCooldownCoroutine()
    {
        isAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isAttackCooldown = false;
    }
}