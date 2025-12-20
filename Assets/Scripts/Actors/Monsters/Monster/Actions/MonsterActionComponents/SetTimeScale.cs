using System;

namespace Actors.Monsters.Actions
{
    internal class SetTimeScale : MonsterActionComponent
    {
        public float TimeScale { get; set; } = 1f;
        public bool ResetOnInterrupt { get; set; } = true;

        public SetTimeScale() { }
        public SetTimeScale(float timeScale, bool resetOnInterrupt = true)
        {
            TimeScale = timeScale;
            ResetOnInterrupt = resetOnInterrupt;

            InterruptPriority = InterruptPriority.Low;
        }

        public SetTimeScale ToDefault()
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
                    $"{nameof(SetTimeScale)} 컴포넌트가 속한 {nameof(MonsterAction)}이(가) 설정되지 않았습니다.");
        }
    }
}
