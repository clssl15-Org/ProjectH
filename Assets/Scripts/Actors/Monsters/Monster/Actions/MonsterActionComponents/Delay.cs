namespace Actors.Monsters.Actions
{
    internal class Delay : MonsterActionComponent
    {
        public float Duration { get; set; }
        private float _startTime;


        public Delay(float duration, bool interruptAllOnDeactivate = false)
        {
            Duration = duration;
            InterruptAllOnDeactivate = interruptAllOnDeactivate;
        }

        protected override void OnEnter(float currentTime, object _)
        {
            _startTime = currentTime;
        }

        protected override void OnUpdate(float elapsedTime)
        {
            if (elapsedTime - _startTime >= Duration)
                Interrupt(InterruptType.Completed);
        }
    }
}
