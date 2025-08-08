using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dokkaebi : Monster
{
    protected override void OnDamaged(int damage)
    {
        print($"데미지 입음: {damage}");
    }

    protected override void OnPlayerDetected(GameObject player)
    {
        print($"플레이어 감지: {player.name}");
    }
}
