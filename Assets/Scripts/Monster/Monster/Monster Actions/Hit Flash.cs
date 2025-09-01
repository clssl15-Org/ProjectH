namespace MonsterActions
{
    internal class HitFlash : MonsteActionState
    {
        private float? mainAnimationLength;
        private ActionResult result;


        public HitFlash(MonsterAction baseAction = MonsterAction.None) : this(baseAction.ToString()) { }
        public HitFlash(string baseAction) : base(MonsterAction.Hit.ToString())
        {
            AnimationName = baseAction.ToString();
        }

        protected override void OnEnter(params object[] inputs)
        {
            base.OnEnter(inputs);

            if (!Owner.StandaloneHitAction.TryHit(out var reason, r => Exit(r)))
            {
                Exit(reason);
                return;
            }

            result = new(ActionResult.ResultType.Interrupted);
        }

        private void Exit(ActionResult result)
        {
            this.result = result;
            Exit();
        }

        protected override void OnExit()
        {
            var callback = Callback;
            Callback = null;

            callback?.Invoke(result);
        }
    }
}
