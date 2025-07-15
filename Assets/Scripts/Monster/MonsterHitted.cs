using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitted : MonoBehaviour
{
    [SerializeField] private MonsterBase monsterBase;

    public void TakeDamage(float damage)
    {
        if (monsterBase.currentState == MonsterBase.State.Dead) return;
        monsterBase.currentHP -= damage;
        monsterBase.ChangeState(MonsterBase.State.Hit);

        if (monsterBase.currentHP <= 0)
        {
            monsterBase.ChangeState(MonsterBase.State.Dead);
        }
    }
}
