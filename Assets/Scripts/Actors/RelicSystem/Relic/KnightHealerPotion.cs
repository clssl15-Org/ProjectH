using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightHealerPotion : Relic
{
    [SerializeField]
    private GameObject potionPrefab;

    private float launchForce = 0.2f;
    public override void OnAcquire()
    {
        // 1. 포션 생성
        GameObject potion = Instantiate(potionPrefab, transform.position, Quaternion.identity);
        Potion potionScript = potion.GetComponent<Potion>();
        potionScript.value = value;

        // 2. 물리 컴포넌트 가져오기
        Rigidbody2D rb = potion.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 3. 랜덤한 X축 값과 일정한 상단 Y축 값 설정
            float randomX = Random.Range(-0.1f, 0.1f);
            float randomY = Random.Range(0.1f, 0.2f);

            Vector2 launchDirection = new Vector2(randomX, randomY).normalized;

            // 4. 힘 가하기 (Impulse 모드는 순간적인 힘을 줄 때 적합합니다)
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

            // 5. 약간의 회전력을 주어 더 자연스럽게 연출
            float randomTorque = Random.Range(-1f, 1f);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }
    }
    protected override void OnLoseCore()
    {
        
    }
}
