using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightHealerPotion : Relic
{
    [SerializeField]
    private GameObject potionPrefab;

    private float launchForce = 5f;
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
            // X는 -1.0 ~ 1.0 사이, Y는 1.5 ~ 2.0 사이로 설정하여 위쪽으로 유도
            float randomX = Random.Range(-1.5f, 1.5f);
            float randomY = Random.Range(1.8f, 2.5f);

            Vector2 launchDirection = new Vector2(randomX, randomY).normalized;

            // 4. 힘 가하기 (Impulse 모드는 순간적인 힘을 줄 때 적합합니다)
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

            // 5. 약간의 회전력을 주어 더 자연스럽게 연출
            float randomTorque = Random.Range(-10f, 10f);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }
    }
    protected override void OnLoseCore()
    {
        
    }
}
