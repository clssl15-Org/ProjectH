using System;

namespace Actors.Monsters.Actions
{
    public enum InterruptType
    {
        None,
        Error,
        Completed,
        Interrupted,
    }

    internal abstract class MonsterActionComponent
    {
        // Front
        public bool Active { get; private set; } = false;
        public bool InterruptAllOnDeactivate { get; set; } = false;
        
        // Internal
        protected MonsterAction MonsterAction { get; private set; }
        protected IMonsterInternal Owner => MonsterAction.Owner;


        // Content
        public virtual void SetParent(MonsterAction monsterAction) => MonsterAction = monsterAction;

        public void Enter(float currentTime, object input = null)
        {
            if (Active)
                return;

            Active = true;
            OnEnter(currentTime, input);
        }
        protected virtual void OnEnter(float currentTime, object input) { }

        public void Update(float elapsedTime) => OnUpdate(elapsedTime);
        protected virtual void OnUpdate(float elapsedTime) { }

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
            InterruptType.Error => ResultType.InvalidOperation,
            InterruptType.Completed => ResultType.Success,
            InterruptType.Interrupted => ResultType.Interrupted,
            _ => throw new InvalidOperationException($"알 수 없는 {nameof(InterruptType)} '{interruptType}'이(가) 감지되었습니다."),
        };
    }
}
