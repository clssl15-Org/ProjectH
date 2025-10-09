namespace MonsterActions
{
    internal class HitFlash : MonsterActionComponent
    {
        public override void Enter(object input)
        {
            Owner.ActionController.StopAnimator();

            if (!Owner.StandaloneHitAction.TryHit(
                reason: out var reason,
                callback: result => Interrupt(result.Result.ToInterruptType())))
            {
                Interrupt(reason.Result.ToInterruptType());
                return;
            }

            base.Enter(input);
        }
    }
}
