using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBase : MonoBehaviour
{
    public enum State { 
        Idle = 0, 
        Detect = 1, 
        Run = 2, 
        Attack = 3, 
        Hit = 4, 
        Dead = 5,
        LAttack = 6,
        RAttack = 7,
    }
    public State currentState = State.Run;

    public float currentHP;
    public float maxHP;

    public void ChangeState(State newState)
    {
        currentState = newState;
    }
}
