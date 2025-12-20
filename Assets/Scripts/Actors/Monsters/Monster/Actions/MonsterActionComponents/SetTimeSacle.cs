using System;

namespace Actors.Monsters.Actions
{
    internal class SetTimeSacle : MonsterActionComponent
    {
        public float TimeScale { get; set; } = 1f;
        public bool ResetOnInterrupt { get; set; } = true;

        public SetTimeSacle() { }
        public SetTimeSacle(float timeScale, bool resetOnInterrupt = true)
        {
            TimeScale = timeScale;
            ResetOnInterrupt = resetOnInterrupt;
        }

        public SetTimeSacle ToDefault()
        {
            TimeScale = 1f;
            return this;
        }

        protected override void OnEnter(object _)
        {
            ThrowIfInvalidParent();
            MonsterAction.TimeScale = TimeScale;
        }

        protected override void OnInterrupt(InterruptType _)
        {
            if (!ResetOnInterrupt)
                return;

            ThrowIfInvalidParent();
            MonsterAction.TimeScale = 1f;
        }

        private void ThrowIfInvalidParent()
        {
            if (MonsterAction == null)
                throw new InvalidOperationException(
                    $"{nameof(SetTimeSacle)} 컴포넌트가 속한 {nameof(MonsterAction)}이(가) 설정되지 않았습니다.");
        }
    }
}
