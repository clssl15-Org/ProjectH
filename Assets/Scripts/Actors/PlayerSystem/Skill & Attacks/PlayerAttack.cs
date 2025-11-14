using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerState playerState;
    [SerializeField] private Transform playerTransform; // Player의 Transform

    [Header("Weapon Objects")] // 공격1,2,3 오브젝트
    [SerializeField] private GameObject[] weaponObjectAttack = new GameObject[3];
    [SerializeField] private GameObject weaponObjectAirAttack;

    private int currentAttackStep = 0; // 현재 공격 단계
    private float comboTimer = 0f;  // 공격 후 콤보 입력 시간
    private float attackTimer = 0f;  // 공격 시간

    [Header("Attack Damage")]
    [SerializeField] private float damageAttack1 = 1.0f; // 콤보 1차 공격 데미지
    [SerializeField] private float damageAttack2 = 1.0f; // 콤보 2차 공격 데미지
    [SerializeField] private float damageAttack3 = 1.5f; // 콤보 3차 공격 데미지
    [SerializeField] private float damageAirAttack = 2.0f; // 공중 공격 데미지
    [SerializeField] private float damageCurrent = 2.0f; // 현재 데미지

    [Header("Combo Time")]
    [SerializeField] private float comboTime = 1f; // 공격 후에 몇 초 안에 다음 공격 눌러야 콤보 이어감? 
    [SerializeField] private float attackTime = 1f; // 공격 유지 시간

    [Header("Animator")]
    [SerializeField] private Animator animator; // 애니메이션

    void Start()
    {
        playerState = PlayerState.Instance; // 플레이어 상태 인스턴스 가져오기

        DisableAttacks();

        //animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 땅에서 공격
        if (Input.GetMouseButtonDown(0) && currentAttackStep < 3 && // 마우스 오른쪽 버튼 공격, 현재 공격 단계가 3타 이하일때
        attackTimer <= 0f && playerState.isGrounded && playerState.IsBasicState())  // 앞선 공격이 끝났을때, 플레이어가 땅에 붙어 있을때. //기본 상태일때
        {
            //animator.SetInteger("AttackStep", currentAttackStep); // 애니메이션 상태 업데이트
            EnableAttack(currentAttackStep);
            currentAttackStep++;
            attackTimer = attackTime;
            comboTimer = -1f; // 콤포 타이머 초기화
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;  // 매 프레임 시간 빼기
            if (attackTimer <= 0f) // 해당 공격 종료
            {
                //animator.SetInteger("AttackStep", 0); // 콤보 종료 또는 다음 공격 대기 상태
                DisableAttacks();
                comboTimer = comboTime; // 콤보 입력 시간 재기.
            }
        }

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;  // 매 프레임 시간 빼기
            if (comboTimer <= 0f)
            {
                DisableAttacks();
                currentAttackStep = 0; // 공격 단계 초기화
            }
        }

        if (playerState.currentState == PlayerState.State.Dashing)
        {   // 대쉬 상태이면 공격 초기화
            DisableAttacksByDash();
            currentAttackStep = 0;
            attackTimer = -1f;
            comboTimer = -1f;
        }



        // 공중에서 공격
        if (Input.GetMouseButtonDown(0) && // 마우스 오른쪽 버튼 공격
            attackTimer <= 0f && !playerState.isGrounded)  // 앞선 공격이 끝났을때, 플레이어가 땅에 붙어 있을때.
        {
            //animator.SetInteger("AttackStep", currentAttackStep); // 애니메이션 상태 업데이트
            EnableAirAttack();  // 공중 공격
            currentAttackStep = 0;
            attackTimer = attackTime;   // 공격 유지 시간
            comboTimer = -1f; // 콤포 타이머 초기화
        }
    }

    void DisableAttacks()    // 공격 비활성화
    {
        weaponObjectAttack[0].SetActive(false);
        weaponObjectAttack[1].SetActive(false);
        weaponObjectAttack[2].SetActive(false);
        weaponObjectAirAttack.SetActive(false);
        playerState.ReturnToBasicState(); // 공격 비활성화 상태(기본상태)로 변경
    }

    void DisableAttacksByDash()    // 공격 대쉬로 비활성화
    {
        weaponObjectAttack[0].SetActive(false);
        weaponObjectAttack[1].SetActive(false);
        weaponObjectAttack[2].SetActive(false);
        weaponObjectAirAttack.SetActive(false);
    }

    // 어택하면 해당 공격 활성화
    public void EnableAttack(int currentAttackStep)
    {
        DisableAttacks();
        weaponObjectAttack[currentAttackStep].SetActive(true);  // 해당 웨폰 오브젝트 활성화
        playerState.ChangeState(PlayerState.State.Attack1 + currentAttackStep);     // 애니메이션, 플레이어 상태 변경

        Transform weaponTransform = weaponObjectAttack[currentAttackStep].GetComponent<Transform>();    // 해당 방향으로 웨폰 위치 바꿈
        weaponTransform.position = playerState.isFacingRight ?     // 플레이어 바라보고 있는 방향에 따라
        new Vector3(playerTransform.position.x - 1f, playerTransform.position.y, 0f) :  // 플레이어 기준 x, y 에 할당
        new Vector3(playerTransform.position.x + 1f, playerTransform.position.y, 0f);

        switch (currentAttackStep)
        {
            case 0:
                damageCurrent = damageAttack1;
                break;
            case 1:
                damageCurrent = damageAttack2;
                break;
            case 2:
                damageCurrent = damageAttack3;
                break;
        }   
    }

    // 공중 공격 활성화
    public void EnableAirAttack()
    {
        DisableAttacks();
        weaponObjectAirAttack.SetActive(true);  // 해당 웨폰 오브젝트 활성화
        playerState.ChangeState(PlayerState.State.AirAttack); // 공중 공격 상태로 변경

        Transform weaponTransform = weaponObjectAirAttack.GetComponent<Transform>();    // 해당 방향으로 웨폰 위치 바꿈
        weaponTransform.position = playerState.isFacingRight ?     // 플레이어 바라보고 있는 방향에 따라
        new Vector3(playerTransform.position.x - 1f, playerTransform.position.y, 0f) :  // 플레이어 기준 x, y 에 할당
        new Vector3(playerTransform.position.x + 1f, playerTransform.position.y, 0f);

        damageCurrent = damageAirAttack;
    }


    // 데미지 반환
    public float GetAttackDamage() {
        return damageCurrent;
    }
}