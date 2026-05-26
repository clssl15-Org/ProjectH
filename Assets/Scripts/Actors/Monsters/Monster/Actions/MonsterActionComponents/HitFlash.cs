namespace Actors.Monsters.Actions
{
    internal class HitFlash : MonsterActionComponent
    {
        protected override void OnEnter(object input)
        {
            Owner.ActionController.StopAnimator();

            if (!Owner.StandaloneHitAction.TryHit(
                reason: out var reason,
                callback: result => Interrupt(result.ResultType.ToInterruptType()),
                allowRestart: true))
            {
                Interrupt(reason.ResultType.ToInterruptType());
                return;
            }
        }
    }
}
