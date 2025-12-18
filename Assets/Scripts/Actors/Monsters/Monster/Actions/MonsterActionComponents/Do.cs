using System;

namespace Actors.Monsters.Actions
{
    internal class Do : MonsterActionComponent
    {
        public bool ImmediateInterrupt { get; set; }
        private Action _opening;

        public Do(bool immediateInterrupt) => ImmediateInterrupt = immediateInterrupt;
        public Do(bool immediateInterrupt, Action opening) : this(immediateInterrupt) =>
            OnOpening(opening);

        public Do OnOpening(Action opening)
        {
            _opening += opening;
            return this;
        }

        protected override void OnEnter(object _)
        {
            _opening?.Invoke();

            if (ImmediateInterrupt)
                Interrupt(InterruptType.Completed);
        }
    }
}
