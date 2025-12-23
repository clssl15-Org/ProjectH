using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Belia
    {
        private class BeliaDashAttackAction : MonsterActionComponent
        {
            public float DashStartTime { get; set; }
            public float DashForce { get; set; }

            public BeliaDashAttackAction(float dashStartTime = 0, float dashForce = 1000)
            {
                DashStartTime = dashStartTime;
                DashForce = dashForce;
            }

            protected override void OnEnter(object _)
            {
                Owner.Rigidbody.AddForce(
                    DashForce * Owner.Direction.ToVector2());
            }

            protected override void OnInterrupt(InterruptType _)
            {
                Owner.Rigidbody.velocity = Vector2.zero;
            }
        }
    }
}
