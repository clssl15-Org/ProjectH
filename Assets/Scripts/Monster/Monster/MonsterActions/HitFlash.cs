namespace MonsterActions
{
    internal class HitFlash : MonsterActionComponent
    {
        public override void Enter(object input)
        {
            if (!Owner.StandaloneHitAction.TryHit(out var reason, r => Interrupt(r.Result.ToInterruptType())))
            {
                Interrupt(reason.Result.ToInterruptType());
                return;
            }

            base.Enter(input);
        }
    }
}
