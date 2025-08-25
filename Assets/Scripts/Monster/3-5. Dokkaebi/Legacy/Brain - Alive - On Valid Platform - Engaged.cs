using UnityEngine;
using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class Engaged : Work<OnValidPlatform>
        {
            // Front
            public float TargetAttackRange { get; set; } = 3f;
            public float UpperRangeTolerance { get; set; } = 0.1f;
            public float LowerRangeTolerance { get; set; } = 0.3f;

            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            protected override void OnUpdate()
            {
                if (!Parent.CheckPlayer(out var player))
                {
                    Parent.SetNextWith<Idle>(true);
                    return;
                }


                var posDelta = Dokkaebi.transform.position.x - player.transform.position.x;
                Dokkaebi.Direction = posDelta > 0 ? Direction.Left : Direction.Right;

                var rangeDelta = Mathf.Abs(posDelta) - TargetAttackRange;

                if (rangeDelta < -LowerRangeTolerance)
                {
                    Dokkaebi.TryMove(posDelta > 0 ? Direction.Right : Direction.Left);
                    return;
                }
                if (rangeDelta > UpperRangeTolerance)
                {
                    Dokkaebi.TryMove();
                    return;
                }

                Parent.SetNext<Attack>();
            }
        }
    }
}
