using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class Belia
    {
        private class BeliaDashAttackAction : MonsterActionComponent
        {
            public float DashStartTime { get; set; }
            public float DashForce { get; set; }

            private Timer _timer;

            public BeliaDashAttackAction(float dashStartTime = 0, float dashForce = 1000)
            {
                DashStartTime = dashStartTime;
                DashForce = dashForce;
            }

            protected override void OnEnter(object _)
            {
                _timer = new(DashStartTime, succeed =>
                {
                    if (succeed)
                    {
                        Owner.Rigidbody.AddForce(
                            DashForce * Owner.Direction.ToVector2());
                    }
                });
            }

            protected override void OnInterrupt(InterruptType _)
            {
                Owner.Rigidbody.velocity = Vector2.zero;

                _timer?.Dispose();
                _timer = null;
            }
        }
    }
}
