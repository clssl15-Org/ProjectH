using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actors.Monsters;

public class PlayerAttackData : MonoBehaviour
{
    [SerializeField] private int damage = 1; // 이 무기의 기본 데미지

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 몬스터 태그를 가진 오브젝트와 충돌했을 때
        if (other.CompareTag("Monster"))
        {
            MonsterDamageReceiver monster = other.GetComponent<MonsterDamageReceiver>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // 무기의 데미지를 몬스터에게 전달
            }
        }
    }
}
