namespace MonsterActions
{
    internal class HitFlash : MonsterAction
    {
        private float? _mainAnimationLength;
        private ActionResult _result;


        public HitFlash(MonsterActionType baseAction = MonsterActionType.None) : this(baseAction.ToString()) { }
        public HitFlash(string baseAction) : base(MonsterActionType.Hit.ToString())
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
            var callback = _callback;
            _callback = null;

            callback?.Invoke(_result);
        }
    }
}
