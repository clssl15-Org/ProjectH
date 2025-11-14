using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerState playerState;
    private PlayerState.State beforeState;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerState = PlayerState.Instance;
        beforeState = playerState.currentState;
    }

    // Update is called once per frame
    void Update()
    {
        if (beforeState != playerState.currentState && beforeState != PlayerState.State.Dead) // Dead 상태가 아니면
        {
            animator.SetInteger("PlayerState", (int)playerState.currentState);
            beforeState = playerState.currentState;
            animator.SetTrigger("PlayerStateTrigger"); // 트리거를 사용하여 애니메이션을 재생
        }
    }
}
