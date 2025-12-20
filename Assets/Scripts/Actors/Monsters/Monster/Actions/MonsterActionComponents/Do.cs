using System;

namespace Actors.Monsters.Actions
{
    internal class Do : MonsterActionComponent
    {
        public bool ImmediateInterrupt { get; set; }
        private Action _opening;
        private Action<InterruptType> _interrupted;

        public Do(bool immediateInterrupt) => ImmediateInterrupt = immediateInterrupt;
        public Do(bool immediateInterrupt, Action opening) : this(immediateInterrupt) =>
            OnOpening(opening);

        public Do OnOpening(Action action)
        {
            _opening += action;
            return this;
        }

        public Do OnInterrupted(Action<InterruptType> action)
        {
            _interrupted += action;
            return this;
        }

        public Do SetInterruptPriotiy(InterruptPriority priority)
        {
            InterruptPriority = priority;
            return this;
        }

        protected override void OnEnter(object _)
        {
            _opening?.Invoke();

            if (ImmediateInterrupt)
                Interrupt(InterruptType.Completed);
        }

        protected override void OnInterrupt(InterruptType reason)
        {
            _interrupted?.Invoke(reason);
        }
    }
}
