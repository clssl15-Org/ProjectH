using System;
using static ActionResult;
using static MonsterActions.MonsterActionComponent;

namespace MonsterActions
{
    internal abstract class MonsterActionComponent
    {
        // Front
        public bool Active { get; private set; } = false;
        public bool InterruptAllOnDeactivate { get; set; } = false;
        
        // Internal
        protected MonsterAction MonsterAction { get; private set; }
        protected IMonster Owner => MonsterAction.Owner;


        // Content
        public virtual void SetParent(MonsterAction monsterAction) => MonsterAction = monsterAction;

        public void Enter(object input = null)
        {
            if (Active)
                return;

            Active = true;
            OnEnter(input);
        }
        protected virtual void OnEnter(object input) { }

        public void Update(float elapsedTime) => OnUpdate(elapsedTime);
        protected virtual void OnUpdate(float elapsedTime) { }

        public enum InterruptType
        {
            None,
            Timeover,
            Error,
            Completed,
            Interrupted,
        }

        public void Interrupt(InterruptType reason)
        {
            if (!Active)
                return;

            Active = false;
            OnInterrupt(reason);
        }
        protected virtual void OnInterrupt(InterruptType reason) { }
    }

    internal static class InterruptTypeExtensions
    {
        public static InterruptType ToInterruptType(this ResultType resultType) => resultType switch
        {
            ResultType.Success => InterruptType.Completed,
            ResultType.AlreadyDoing | ResultType.OtherActionDoing | ResultType.NotFound => InterruptType.Error,
            ResultType.Interrupted => InterruptType.Interrupted,
            ResultType.InvalidOperation => InterruptType.Error,
            _ => throw new InvalidOperationException($"알 수 없는 {nameof(ResultType)} '{resultType}'이(가) 감지되었습니다."),
        };
        public static ResultType ToResultType(this InterruptType interruptType) => interruptType switch
        {
            InterruptType.None => ResultType.Interrupted,
            InterruptType.Timeover => ResultType.Success,
            InterruptType.Error => ResultType.InvalidOperation,
            InterruptType.Completed => ResultType.Success,
            InterruptType.Interrupted => ResultType.Interrupted,
            _ => throw new InvalidOperationException($"알 수 없는 {nameof(InterruptType)} '{interruptType}'이(가) 감지되었습니다."),
        };
    }
}
