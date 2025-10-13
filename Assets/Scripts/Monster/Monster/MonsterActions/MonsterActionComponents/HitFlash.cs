namespace MonsterActions
{
    internal class HitFlash : MonsterActionComponent
    {
        protected override void OnEnter(object _)
        {
            Owner.ActionController.StopAnimator();

            if (!Owner.StandaloneHitAction.TryHit(
                reason: out var reason,
                callback: result => Interrupt(result.Result.ToInterruptType())))
            {
                Interrupt(reason.Result.ToInterruptType());
                return;
            }
        }
    }
}
