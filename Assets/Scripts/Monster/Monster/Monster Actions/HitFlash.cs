namespace MonsterActions
{
    internal class HitFlash : MonsterActionState
    {
        private float? _mainAnimationLength;
        private ActionResult _result;


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

            _result = new(ActionResult.ResultType.Interrupted);
        }

        private void Exit(ActionResult result)
        {
            _result = result;
            Exit();
        }

        protected override void OnExit()
        {
            var callback = Callback;
            Callback = null;

            callback?.Invoke(_result);
        }
    }
}
