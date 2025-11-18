using UnityEngine;
using System.Collections.Generic;

/* 
 * 이 파일은 MonsterSpawner에서 사용할 데이터 구조(Data Structures)만 정의합니다.
 */

/// <summary>
/// 몬스터 스폰의 단일 '페이즈(Phase)'를 정의하는 데이터 클래스입니다.
/// </summary>
public class SpawnPhase
{
    public int phaseNumber;

    public List<GameObject> fixedMonsterPool;

    public RandomPoolSettings randomMonsterPool;
}

/// <summary>
/// '랜덤 스폰 방식'의 세부 규칙을 정의하는 데이터 클래스입니다.
/// </summary>
public class RandomPoolSettings
{
    public int spawnCount;

    public List<GameObject> monsterPrefabs;
}