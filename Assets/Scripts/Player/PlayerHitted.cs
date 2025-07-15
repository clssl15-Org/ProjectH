using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitted : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody; // Player의 Rigidbody2D
    [SerializeField] private Transform playerTransform; // Player의 Transform
    [SerializeField] private SpriteRenderer playerBodySpriteRenderer;   // 플레이어바디의 spriteRenderer
    private PlayerState playerState;
    private PlayerManger playerManger;

    [Header("Hit Reaction Settings")] // 피격 관련 설정정
    [SerializeField] private float invincibleTime = 1f;     // 피격 후 무적 지속 시간
    [SerializeField] private float inHittedTime = 0.5f;     // 피격 후 경직 시간
    [SerializeField] private float hittedXPower = 5f;     // 피격 후 y축 이동
    [SerializeField] private float hittedYPower = 5f;     // 피격 후 x축 이동

    // Start is called before the first frame update
    void Start()
    {
        playerState = PlayerState.Instance; // 플레이어 상태 인스턴스 가져오기
        playerManger = PlayerManger.Instance; // 플레이어 매니저 인스턴스 가져오기 HP 관리 용
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 무적 상태가 아니고 충돌한 오브젝트의 태그가 "MonsterAttack"라면  
        if (!playerState.isInvincible && other.CompareTag("MonsterAttack") && !playerState.isDead)
        {
            if (other.gameObject.transform.IsChildOf(transform))
            {
                return;
            }

            // 피격 처리 시작
            HitReaction(other.transform);
        }
    }

    public void TakeDamage(float damage)        // 플레이어가 받은 데미지 처리 (MonsterAttackData에서 호출)
    {
        if (playerState.isInvincible || playerState.isDead) return;
        playerManger.DecreaseCurrentHP(damage);
    }



    private void HitReaction(Transform attacker)
    {
        // 무적 상태로 만듦
        StartCoroutine(InvincibleCoroutine());

        // 경직시간 시작
        StartCoroutine(HittedCoroutine());

        playerRigidbody.velocity = Vector2.zero; // 현재 속도를 초기화

        // 맞은 x 축 방향의 반대 방향을 구함
        int knockbackDirectionX = attacker.position.x < playerTransform.position.x ? 1 : -1;
        playerRigidbody.AddForce(new Vector2(knockbackDirectionX * hittedXPower, hittedYPower), ForceMode2D.Impulse);
    }


    IEnumerator InvincibleCoroutine()
    { // 피격 무적 코루틴
        playerState.isInvincible = true; // 무적 상태
        playerBodySpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(invincibleTime); // 설정한 시간 동안 대기
        playerState.isInvincible = false; // 무적x 상태
        playerBodySpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    IEnumerator HittedCoroutine()
    { // 피격 경직직 코루틴
        playerState.ChangeState(PlayerState.State.Hitted); // 경직 상태
        yield return new WaitForSeconds(inHittedTime); // 설정한 경직직 시간 동안 대기
        playerState.ReturnToBasicState(); // 경직 끝, 기본 상태로 변경
    }
}
