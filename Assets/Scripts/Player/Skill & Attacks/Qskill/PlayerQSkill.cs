using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQSkill : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform playerTransform;
    private PlayerState playerState;
    private PlayerManger playerManger;

    [Header("QSkill Objects")] // QSkill 오브젝트
    [SerializeField] private GameObject QSkillObject;

    [Header("QSkill settings")] // ESkill 설정
    [SerializeField] private float QSkillDamage = 20f; // 데미지 배수
    [SerializeField] private float QSkillCharge = 2f; // Q 스킬 게이지 충전량   // 공격 1타당 차지
    [SerializeField] private float QSkillMaxCharge = 100f; // Q 스킬 발동 조건
    [SerializeField] private float QSkillDuration = 2f; // Q스킬 애니메이션 유지 시간

    [SerializeField] private float QSkillGauge = 0f; // Q스킬 게이지
    [SerializeField] private float QSkillDamageDelay = 0.5f; // Q스킬 데미지 지연 시간

    // Start is called before the first frame update
    void Start()
    {
        // ESkill 오브젝트 비활성화
        QSkillObject.SetActive(false);

        // 플레이어 상태 가져오기
        playerState = PlayerState.Instance;

        // 플레이어 매니저 가져오기
        playerManger = PlayerManger.Instance;

        QSkillGauge = 0;    // q스킬 게이지 처음에 0

        // 플레이어 매니저 Q 관련 변수 초기화ㅣ
        playerManger.currentQSkillGauge = QSkillGauge;  // 현재 Q 스킬 게이지
        playerManger.maxQSkillGauge = QSkillMaxCharge;  // max Q 스킬 게이지
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && playerState.IsBasicState() && playerState.isGrounded // q를 누르고, 기본 상태이고 땅에 붙어 있을때
        && QSkillGauge >= QSkillMaxCharge)  // 게이지가 다 찼을때
        {
            StartCoroutine(QSkillCoroutine());
            QSkillGauge = 0;   // q스킬 게이지 초기화
        }
        playerManger.currentQSkillGauge = QSkillGauge;  // 현재 Q 스킬 게이지 플레이어 매니저에 업데이트
    }

    private IEnumerator QSkillCoroutine()
    {


        playerState.ChangeState(PlayerState.State.QSkill); // q스킬 상태로 변경
        StartCoroutine(EnableQSkill()); // q스킬 활성화

        playerState.isSkillActive = true; // 스킬 활성화
        playerState.isInvincible = true; // 무적 상태 활성화
        playerRigidbody.velocity = Vector2.zero;    // 플레이어 제자리에 멈추게 만들기



        float timer = 0f;   // Q 스킬 타이머
        while (timer < QSkillDuration)
        {
            playerRigidbody.velocity = Vector2.zero; // 매 프레임 멈춤 상태 유지

            // 필요하다면 매 프레임 QSkill 상태를 확인하거나 다른 동작 수행
            if (playerState.currentState != PlayerState.State.QSkill)
            {
                playerState.ChangeState(PlayerState.State.QSkill);
            }

            yield return null; // 다음 프레임까지 대기
            timer += Time.deltaTime;
        }

        playerState.isSkillActive = false; // 스킬 비활성화

        playerState.ReturnToBasicState(); // e스킬 상태 종료, 기본 상태로 변경
        DisableQSkill(); // q스킬 비활성화
        
        playerState.isInvincible = false; // 무적 상태 비활성화
    }


    // QSkill 활성화
    public IEnumerator EnableQSkill()
    {
        DisableQSkill();
        yield return new WaitForSeconds(QSkillDamageDelay); // Q스킬 데미지 지연 시간
        QSkillObject.SetActive(true);  // 해당 웨폰 오브젝트 활성화

        Transform QSkillTransform = QSkillObject.GetComponent<Transform>();    // 해당 방향으로 웨폰 위치 바꿈
        QSkillTransform.position = playerState.isFacingRight ?     // 플레이어 바라보고 있는 방향에 따라
        new Vector3(playerTransform.position.x - QSkillTransform.localScale.x * 3 / 7, playerTransform.position.y, 0f) :  // 플레이어 기준 x, y 에 할당
        new Vector3(playerTransform.position.x + QSkillTransform.localScale.x * 3 / 7, playerTransform.position.y, 0f);
    }

    private void DisableQSkill()
    {
        QSkillObject.SetActive(false);  // 해당 웨폰 오브젝트 비활성화
    }


    public void IncreaseQSkillGauge()
    {     // Q스킬 증가
        if (QSkillGauge < QSkillMaxCharge)
        {    // 게이지가 다 안 찼으면
            QSkillGauge += QSkillCharge;        // 게이지에 게이지 증가량 더해주기
        }
    }

    public float GetQSkillGauge()
    {     // q스킬 게이지 가져오기
        return QSkillGauge;
    }



    // 데미지 반환
    public float GetDamage()
    {
        return QSkillDamage;
    }
}

