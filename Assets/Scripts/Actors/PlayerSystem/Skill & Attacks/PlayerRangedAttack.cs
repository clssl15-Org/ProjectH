using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRangedAttack : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform playerTransform;
    private PlayerState playerState;

    [Header("RangedAttack Objects")] // RangedAttack 오브젝트
    [SerializeField] private GameObject RangedAttackObject;

    [Header("RangedAttack settings")] // RangedAttack 설정
    [SerializeField] private float RangedAttackDamage = 3f; // 데미지 배수
    [SerializeField] private float RangedAttackCooldown = 5f; // 쿨타임
    [SerializeField] private float RangedAttackDistance = 50f; // 사거리
    [SerializeField] private float RangedAttackSpeed = 5f; // 속도
    [SerializeField] private float RangedAttackDuration = 0.5f; // 유지 시간(원거리 공격 애니메이션 활성화 시간)

    private float lastRangedAttackTime = 0;   // 마지막 사용 시간
    void Start()
    {


        RangedAttackObject.SetActive(false);    // 원거리공격 오브젝트 비활성화
        playerState = PlayerState.Instance;
        lastRangedAttackTime = -RangedAttackCooldown; // 쿨타임 초기화
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && playerState.IsBasicState())  // 마우스 오른쪽 버튼을 누르고, 기본 상태일때
        {
            StartCoroutine(RangedAttackCoroutine());
        }
    }

    private IEnumerator RangedAttackCoroutine()
    {
        // 스킬 쿨타임 체크
        if (Time.time - lastRangedAttackTime < RangedAttackCooldown)
        {
            yield break;
        }
        lastRangedAttackTime = Time.time; // 마지막 사용 시간 업데이트

        playerState.ChangeState(PlayerState.State.RangedAttack); // 원거리 공격 상태로 변경
        StartCoroutine(EnableRangedAttack()); // 원거리 공격 활성화


        float startTime = Time.time; // 코루틴 시작 시간 기록

        while (Time.time - startTime < RangedAttackDuration) // 원거리 공격 애니메이션 활성화 시간 동안 대기
        {
            if (playerState.currentState == PlayerState.State.Dashing)
            {
                yield break; // 코루틴 애니메이션 종료
            }
            yield return null; // 매 프레임마다 상태 확인
        }

        playerState.ReturnToBasicState(); // 원거리 공격 상태 종료, 기본 상태로 변경
    }


    // RangedAttack 활성화
    private IEnumerator EnableRangedAttack()
    {
        // 1. 자신을 복제하여 새로운 공격 오브젝트 생성
        GameObject newRangedAttackObject = Instantiate(RangedAttackObject);
        newRangedAttackObject.SetActive(true); // 복제된 오브젝트 활성화

        // 복제된 오브젝트의 SpriteRenderer의 flipX 설정
        SpriteRenderer rangedAttackSpriteRenderer = newRangedAttackObject.GetComponent<SpriteRenderer>();
        rangedAttackSpriteRenderer.flipX = playerState.isFacingRight;

        // 2. 복제된 오브젝트의 Transform 컴포넌트 가져오기
        Transform rangedAttackTransform = newRangedAttackObject.GetComponent<Transform>();

        // 3. 시작 방향 및 위치 설정 (복제된 오브젝트 기준)
        bool startFlipX = playerState.isFacingRight;
        rangedAttackTransform.position = startFlipX ?
            new Vector3(playerTransform.position.x - 0.5f, playerTransform.position.y, 0f) :
            new Vector3(playerTransform.position.x + 0.5f, playerTransform.position.y, 0f);


        rangedAttackTransform.parent = null; // 복제된 오브젝트의 부모를 없애서 복제된 오브젝트가 플레이어 오브젝트와 같이 움직이지 않도록 함

        // 4. 끝 위치 지정
        Vector3 endPosition = startFlipX ?
            new Vector3(rangedAttackTransform.position.x - RangedAttackDistance, rangedAttackTransform.position.y, 0f) :
            new Vector3(rangedAttackTransform.position.x + RangedAttackDistance, rangedAttackTransform.position.y, 0f);

        // 5. 끝 위치까지 이동
        while (startFlipX ? rangedAttackTransform.position.x > endPosition.x
            : rangedAttackTransform.position.x < endPosition.x)
        {
            rangedAttackTransform.position = Vector3.MoveTowards(rangedAttackTransform.position, endPosition, RangedAttackSpeed * Time.deltaTime);
            yield return null;
        }

        // 6. 목적지에 도착하면 복제된 오브젝트 파괴
        Destroy(newRangedAttackObject);
    }


    // 데미지 반환
    public float GetDamage() {
        return RangedAttackDamage;
    }
}
