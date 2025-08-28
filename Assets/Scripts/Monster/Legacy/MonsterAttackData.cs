using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackData : MonoBehaviour
{
    [SerializeField] private float damage = 1f; // 이 무기의 기본 데미지

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그를 가진 오브젝트와 충돌했을 때
        if (other.CompareTag("Player"))
        {
            PlayerHitted player = other.GetComponent<PlayerHitted>();
            if (player != null)
            {
                player.TakeDamage(damage); // 무기의 데미지를 플레이어에게 전달
            }
        }
    }
}
