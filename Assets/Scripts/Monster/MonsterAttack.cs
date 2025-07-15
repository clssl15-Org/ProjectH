using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeetleAttack()
    {
        // 풍뎅이 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("풍뎅이 공격 실행!");
    }
    public void WoodAttack()
    {
        // 나무몹 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("나무 공격 실행!");
    }
    public void WoodLAttack()
    {
        // 나무몹 땅 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("나무 땅 공격 실행!");
    }

    public void DokebiAttack()
    {
        // 도깨비 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("도깨비 공격 실행!");
    }

    public void TransAttack()
    {
        // 변신수 땅 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("변신수 공격 실행!");
    }
    public void DeerAttack()
    {
        // 사슴벌레 땅 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("사슴 공격 실행!");
    }
    public void DeerLAttack()
    {
        // 사슴벌레 돌진 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("사슴 돌진 공격 실행!");
    }
    public void DeerRAttack()
    {
        // 사슴벌레 원거리 공격 로직 (예: 플레이어에게 데미지 주기, 이펙트 등)
        Debug.Log("사슴 원거리 공격 실행!");
    }
}
