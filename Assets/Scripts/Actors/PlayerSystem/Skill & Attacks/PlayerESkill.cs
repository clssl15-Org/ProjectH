using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerESkill : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform playerTransform;
    private PlayerState playerState;

    [Header("ESkill Objects")] // ESkill 오브젝트
    [SerializeField] private GameObject ESkillObject;

    [Header("ESkill settings")] // ESkill 설정
    [SerializeField] private float ESkillDamage = 5f; // 데미지 배수
    [SerializeField] private float ESkillCooldown = 7f; // 쿨타임
    [SerializeField] private float ESkillDistance = 3.5f; // 돌진 거리
    [SerializeField] private float ESkillDuration = 2f; // e스킬 유지 시간
    [SerializeField] private float ESkillSpeed = 10f; // e스킬 속도

    private float lastESkillTime = 0;   // e스킬 마지막 사용 시간

    // Start is called before the first frame update
    void Start()
    {
        // ESkill 오브젝트 비활성화
        ESkillObject.SetActive(false);

        // 플레이어 상태 가져오기
        playerState = PlayerState.Instance;

        // 마지막 e스킬 사용 시간 초기화
        lastESkillTime = -ESkillCooldown; // 쿨타임 초기화
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerState.IsBasicState())  // e를 누르고, 기본 상태일때
        {
            StartCoroutine(ESkillCoroutine());
        }
    }

    private IEnumerator ESkillCoroutine()
    {
        // 스킬 쿨타임 체크
        if (Time.time - lastESkillTime < ESkillCooldown)
        {
            yield break;
        }
        lastESkillTime = Time.time; // 마지막 사용 시간 업데이트

        playerState.ChangeState(PlayerState.State.ESkill); // e스킬 상태로 변경
        EnableESkill(); // e스킬 활성화

        playerState.isInvincible = true; // 무적 상태 활성화


        float startPosition = transform.position.x; // e대쉬 시작 위치 저장

        float eDashDirection; // e찌르기 방향
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f)
        {    // 누르고 있는 버튼이 없다면.
            eDashDirection = playerState.isFacingRight ? -1f : 1f; // 바라보는 방향에 따라 대쉬 방향 결정
        }
        else
        {
            eDashDirection = Mathf.Sign(Input.GetAxis("Horizontal"));    // 누르고 있는 방향으로 대쉬 방향 결정
        }


        // 처음 e대쉬 (아무것도 안 누른 상태에서도 e대쉬 되게)  , 유지 시간 체크
        while (Mathf.Abs(transform.position.x - startPosition) < 0.01f
        && Time.time - lastESkillTime < ESkillDuration)
        {
            playerRigidbody.velocity = new Vector2(eDashDirection * ESkillSpeed, playerRigidbody.velocity.y);  // 대쉬
            yield return null; // 프레임별로 업데이트
            if (Mathf.Abs(playerRigidbody.velocity.x) < 0.01f) break; // 벽에 부딪혀서 속도가 0에 가까워졌을때 대쉬가 멈추게
        }

        // e대쉬 실행        
        while (Mathf.Abs(transform.position.x - startPosition) < ESkillDistance
        && Time.time - lastESkillTime < ESkillDuration) // 거리를 설정한 만큼 가거나 유지 시간 체크
        {
            playerRigidbody.velocity = new Vector2(eDashDirection * ESkillSpeed, playerRigidbody.velocity.y);  // 대쉬
            yield return null; // 프레임별로 업데이트       
            if (Mathf.Abs(playerRigidbody.velocity.x) < 0.01f) break; // 벽에 부딪혀서 속도가 0에 가까워졌을때 대쉬가 멈추게

            if (playerState.currentState == PlayerState.State.Dashing)
            {
                DisableESkill();
                yield break; // 대쉬 상태일때 e스킬 종료
            }
        }

        playerState.ReturnToBasicState(); // e스킬 상태 종료, 기본 상태로 변경
        DisableESkill(); // e스킬 비활성화

        playerState.isInvincible = false; // 무적 상태 비활성화
    }


    // ESkill 활성화
    public void EnableESkill()
    {
        DisableESkill();
        ESkillObject.SetActive(true);  // 해당 웨폰 오브젝트 활성화

        Transform ESkillTransform = ESkillObject.GetComponent<Transform>();    // 해당 방향으로 웨폰 위치 바꿈
        ESkillTransform.position = playerState.isFacingRight ?     // 플레이어 바라보고 있는 방향에 따라
        new Vector3(playerTransform.position.x - 1f, playerTransform.position.y, 0f) :  // 플레이어 기준 x, y 에 할당
        new Vector3(playerTransform.position.x + 1f, playerTransform.position.y, 0f);
    }

    private void DisableESkill()
    {
        ESkillObject.SetActive(false);  // 해당 웨폰 오브젝트 비활성화
    }


    // 데미지 반환
    public float GetDamage() {
        return ESkillDamage;
    }
}
