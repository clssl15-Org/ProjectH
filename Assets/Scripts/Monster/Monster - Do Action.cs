using System;
using UnityEngine;

public abstract partial class Monster
{
    public bool TryDoAction(MonsterAction action, Action callback = null)
    {
        DoNone();

        return action switch
        {
            MonsterAction.Idle => DoIdle(),
            MonsterAction.Alert => DoAlert(),
            MonsterAction.Blink => DoBlikn(),
            MonsterAction.Walk => DoWalk(),
            MonsterAction.Run => DoRun(),
            MonsterAction.Attack => DoAttack(callback),
            MonsterAction.Hit => DoHit(callback),
            MonsterAction.Dead => DoDead(callback),
            MonsterAction.DeadImpact => DoDeadImpact(callback),
            _ => DoNone(),
        };
    }


    protected virtual bool DoIdle()
    {
        animator?.Play(MonsterAction.Idle.ToString());
        return true;
    }

    protected virtual bool DoAlert()
    {
        animator?.Play(MonsterAction.Alert.ToString());
        return true;
    }

    protected virtual bool DoBlikn()
    {
        animator?.Play(MonsterAction.Blink.ToString());
        return true;
    }

    protected virtual bool DoWalk()
    {
        animator?.Play(MonsterAction.Walk.ToString());
        return true;
    }

    protected virtual bool DoRun()
    {
        animator?.Play(MonsterAction.Run.ToString());
        return true;
    }


    private bool isAttacking = false;

    protected virtual bool DoAttack(Action callback)
    {
        if (isAttacking) return false;
        isAttacking = true;

        if (HasAnimator)
        {
            var name = MonsterAction.Attack.ToString();

            animator.Play(name);
            AnimatorCallback += Callback;

            void Callback(AnimatorStateInfo stateInfo, bool isEnter)
            {
                if (!stateInfo.IsName(name) || isEnter)
                    return;

                isAttacking = false;
                callback?.Invoke();

                AnimatorCallback -= Callback;
            }
        }

        return true;
    }


    private bool isUnderHit = false;

    protected virtual bool DoHit(Action callback)
    {
        if (isUnderHit) return false;
        isUnderHit = true;

        if (HasAnimator)
        {
            var name = MonsterAction.Hit.ToString();

            animator.Play(name);
            AnimatorCallback += Callback;

            void Callback(AnimatorStateInfo stateInfo, bool isEnter)
            {
                if (!stateInfo.IsName(name) || isEnter)
                    return;

                isUnderHit = false;
                callback?.Invoke();

                AnimatorCallback -= Callback;
            }
        }

        return true;
    }


    private bool isDying = false;

    protected virtual bool DoDead(Action callback)
    {
        if (isDying) return false;
        isDying = true;

        if (HasAnimator)
        {
            var name = MonsterAction.Dead.ToString();

            animator.Play(name);
            AnimatorCallback += Callback;

            void Callback(AnimatorStateInfo stateInfo, bool isEnter)
            {
                if (!stateInfo.IsName(name) || isEnter)
                    return;

                isDying = false;
                callback?.Invoke();

                AnimatorCallback -= Callback;
            }
        }

        return true;
    }

    protected virtual bool DoDeadImpact(Action callback)
    {
        if (isDying) return false;
        isDying = true;

        if (HasAnimator)
        {
            var name = MonsterAction.DeadImpact.ToString();

            animator.Play(name);
            AnimatorCallback += Callback;

            void Callback(AnimatorStateInfo stateInfo, bool isEnter)
            {
                if (!stateInfo.IsName(name) || isEnter)
                    return;

                isDying = false;
                callback?.Invoke();

                AnimatorCallback -= Callback;
            }
        }

        return true;
    }

    protected virtual bool DoNone()
    {
        animator?.StopPlayback();
        return true;
    }
}
