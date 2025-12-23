namespace Actors.Monsters.Actions
{
    internal class Delay : MonsterActionComponent
    {
        public float Duration { get; set; }
        private float _elapsedTime;


        public Delay(float duration, InterruptPriority interruptPriority = InterruptPriority.Default)
        {
            Duration = duration;
            InterruptPriority = interruptPriority;
        }

        protected override void OnEnter(object _)
        {
            _elapsedTime = 0f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _elapsedTime += deltaTime;

            if (_elapsedTime >= Duration)
                Interrupt(InterruptType.Completed);
        }
    }
}
