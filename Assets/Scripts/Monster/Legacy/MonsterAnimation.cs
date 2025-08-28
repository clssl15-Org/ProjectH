using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    private Animator animator;
    //[SerializeField] private MonsterBase MonsterAI;
    //[SerializeField] private MonsterBase.State beforeState;

    private void Awake()
    {
        //animator = GetComponent<Animator>();
        //beforeState = MonsterAI.currentState;
    }

    // Update is called once per frame
    void Update()
    {
        //if (beforeState != MonsterAI.currentState && beforeState != MonsterBase.State.Dead)
        //{
        //    // Debug.Log("MonsterAnimation Update");
        //    // Debug.Log("MonsterAI.currentState: " + (int)MonsterAI.currentState);
        //    animator.SetInteger("MonsterState", (int)MonsterAI.currentState);
        //    beforeState = MonsterAI.currentState;
        //    animator.SetTrigger("MonsterStateTrigger"); // 트리거를 사용하여 애니메이션을 재생
        //}
    }
}
