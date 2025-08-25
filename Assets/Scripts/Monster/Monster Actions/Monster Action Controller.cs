using System;
using UniEngine.StateMachines.FSM;

namespace MonsterActions
{
    internal abstract class MonsterActionController : Work
    {
        public Monster Owner { get; protected set; }
        public float DefaultCallbackToleranceTime { get; set; } = 0f;

        public MonsterActionController(Monster monster)
        {
            Owner = monster;
        }

        public bool TryDoAction(
            string monsterAction,
            out ActionResult reason,
            Action<ActionResult> callback = null,
            bool stopPreviousAction = true,
            bool allowRestart = false,
            float? playTime = null)
        {
            if (TryGetCurrentChild<MonsteActionState>(out var current))
            {
                if (current.Name != monsterAction)
                {
                    if (!stopPreviousAction)
                    {
                        reason = new(ActionResult.ResultType.OtherActionExecuting,
                            $"이미 다른 행동({current.Name})이 실행 중이기 때문에 입력한 행동({monsterAction})을 실행할 수 없습니다.");

                        return false;
                    }
                }
                else
                {
                    if (!allowRestart)
                    {
                        reason = new(ActionResult.ResultType.AlreadyDoing,
                            $"이미 입력한 행동({monsterAction})이 실행 중입니다.");

                        return false;
                    }
                }
            }


            try
            {
                SetNextWith(monsterAction, callback, playTime);

                reason = new(ActionResult.ResultType.Success);
                return true;
            }
            catch (ArgumentException ex)
            {
                reason = new(ActionResult.ResultType.NotFound,
                    $"입력한 행동 상태({monsterAction})를 찾는 데 실패했습니다.", ex);

                return false;
            }
        }

        public MonsterAction GetCurrentAction()
        {
            if (TryGetCurrentChild<MonsteActionState>(out var child))
            {
                if (child.Name == MonsterAction.Idle.ToString())
                    return MonsterAction.Idle;
                if (child.Name == MonsterAction.Alert.ToString())
                    return MonsterAction.Alert;
                if (child.Name == MonsterAction.Walk.ToString())
                    return MonsterAction.Walk;
                if (child.Name == MonsterAction.Run.ToString())
                    return MonsterAction.Run;
                if (child.Name == MonsterAction.Attack.ToString())
                    return MonsterAction.Attack;
                if (child.Name == MonsterAction.Hit.ToString())
                    return MonsterAction.Hit;
                if (child.Name == MonsterAction.Dead.ToString())
                    return MonsterAction.Dead;

                return MonsterAction.Unknown;
            }

            return MonsterAction.None;
        }

        public void StopCurrentAction() => SetNextToNone();
    }
}
