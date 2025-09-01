using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;
using static ActionResult;

namespace MonsterActions
{
    internal abstract class MonsterActionController : Work
    {
        public Monster Owner { get; protected set; }
        public float DefaultCallbackToleranceTime { get; set; } = 0f;

        private Sprite originalSprite;


        public MonsterActionController(Monster monster)
        {
            Owner = monster;
            originalSprite = Owner.SpriteRenderer.sprite;

            StopAnimator();
        }

        public bool TryDoAction(
            string monsterAction,
            out ActionResult reason,
            Action<ActionResult> callback = null,
            bool stopPreviousAction = true,
            bool allowRestart = false,
            float? playTime = null,
            float stayTimeAfterFinised = 0f)
        {
            if (monsterAction == MonsterAction.None.ToString())
            {
                StopCurrentAction();

                reason = new(ResultType.Success,
                    $"입력한 행동 상태({monsterAction})가 {MonsterAction.None.ToString()}이기 때문에 행동을 하지 않는 상태로 설정하였습니다.");

                return true;
            }

            if (TryGetCurrentChild<MonsteActionState>(out var current))
            {
                if (current.Name != monsterAction)
                {
                    if (!stopPreviousAction)
                    {
                        reason = new(ResultType.OtherActionExecuting,
                            $"이미 다른 행동({current.Name})이 실행 중이기 때문에 입력한 행동({monsterAction})을 실행할 수 없습니다.");

                        return false;
                    }
                }
                else
                {
                    if (!allowRestart)
                    {
                        reason = new(ResultType.AlreadyDoing,
                            $"이미 입력한 행동({monsterAction})이 실행 중입니다.");

                        return false;
                    }
                }
            }


            try
            {
                Owner.Animator.enabled = true;
                SetNextWith(monsterAction, callback, playTime, stayTimeAfterFinised);

                reason = new(ResultType.Success);
                return true;
            }
            catch (ArgumentException ex)
            {
                reason = new(ResultType.NotFound,
                    $"입력한 행동 상태({monsterAction})를 찾는 데 실패했습니다.", ex);

                return false;
            }
        }

        public MonsterAction GetCurrentAction()
        {
            if (TryGetCurrentAction(out var name))
            {
                if (name == MonsterAction.Idle.ToString())
                    return MonsterAction.Idle;
                if (name == MonsterAction.Alert.ToString())
                    return MonsterAction.Alert;
                if (name == MonsterAction.Walk.ToString())
                    return MonsterAction.Walk;
                if (name == MonsterAction.Run.ToString())
                    return MonsterAction.Run;
                if (name == MonsterAction.Attack.ToString())
                    return MonsterAction.Attack;
                if (name == MonsterAction.Hit.ToString())
                    return MonsterAction.Hit;
                if (name == MonsterAction.Dead.ToString())
                    return MonsterAction.Dead;

                return MonsterAction.Undefined;
            }

            return MonsterAction.None;
        }

        public bool TryGetCurrentAction(out string name)
        {
            if (TryGetCurrentChild(out var child))
            {
                name = child.Name;
                return true;
            }

            name = string.Empty;
            return false;
        }

        public void StopCurrentAction()
        {
            SetNextToNone();
            StopAnimator();
        }

        internal void StopAnimator()
        {
            Owner.Animator.enabled = false;
            Owner.SpriteRenderer.sprite = originalSprite;
        }
    }
}
