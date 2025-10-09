using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;
using static ActionResult;

namespace MonsterActions
{
    internal abstract class MonsterActionController : Work
    {
        public Monster Owner { get; }
        private Sprite _originalSprite;


        public MonsterActionController(Monster monster)
        {
            Owner = monster;
            _originalSprite = Owner.SpriteRenderer.sprite;

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
            if (monsterAction == MonsterActionType.None.ToString())
            {
                StopCurrentAction();

                reason = new(ResultType.Success,
                    $"입력한 행동 상태 '{monsterAction}'이(가) {MonsterActionType.None.ToString()}이기 때문에 행동을 하지 않는 상태로 설정하였습니다.");

                return true;
            }

            if (TryGetCurrentChild<MonsterAction>(out var current))
            {
                if (current.Name != monsterAction)
                {
                    if (!stopPreviousAction)
                    {
                        reason = new(ResultType.OtherActionDoing,
                            $"이미 다른 행동 '{current.Name}'이(가) 실행 중이기 때문에 입력한 행동 '{monsterAction}'을(를) 실행할 수 없습니다.");

                        return false;
                    }
                }
                else
                {
                    if (!allowRestart)
                    {
                        reason = new(ResultType.AlreadyDoing,
                            $"이미 입력한 행동 '{monsterAction}'이(가) 실행 중입니다.");

                        return false;
                    }
                }
            }


            try
            {
                SetNextWith(monsterAction, callback, playTime, stayTimeAfterFinised);

                reason = new(ResultType.Success);
                return true;
            }
            catch (ArgumentException ex)
            {
                reason = new(ResultType.NotFound,
                    $"입력한 행동 상태 '{monsterAction}'을(를) 찾는 데 실패했습니다.", ex);

                return false;
            }
        }

        public MonsterActionType GetCurrentAction()
        {
            if (TryGetCurrentAction(out var name))
            {
                if (name == MonsterActionType.Idle.ToString())
                    return MonsterActionType.Idle;
                if (name == MonsterActionType.Alert.ToString())
                    return MonsterActionType.Alert;
                if (name == MonsterActionType.Walk.ToString())
                    return MonsterActionType.Walk;
                if (name == MonsterActionType.Run.ToString())
                    return MonsterActionType.Run;
                if (name == MonsterActionType.Attack.ToString())
                    return MonsterActionType.Attack;
                if (name == MonsterActionType.Hit.ToString())
                    return MonsterActionType.Hit;
                if (name == MonsterActionType.Dead.ToString())
                    return MonsterActionType.Dead;

                return MonsterActionType.Undefined;
            }

            return MonsterActionType.None;
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
            Owner.AnimationPlayer.Stop();
            Owner.SpriteRenderer.sprite = _originalSprite;
        }
    }
}
