using Infrastructure;
using UnityEngine;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Attack : Work<OnValidPlatform>
        {
            // Front
            public float Cooldown { get; set; } = 0.5f;

            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            public Attack()
            {
                AddChild(new DoAttack(), true);
                AddChild(new DoCooldown());
            }

            // Forwarding
            protected bool CheckPlayer(out GameObject player) => Parent.CheckPlayer(out player);


            // Substates
            private class DoAttack : Work<Attack>
            {
                protected override void OnEnter(params object[] _)
                {
                    var attacking = Parent.Dokkaebi.TryAttack(() =>
                    {
                        if (Active)
                            Parent.SetNext<DoCooldown>();
                    });

                    if (!attacking)
                        Parent.SetNext<DoCooldown>();
                }

                protected override void OnExit()
                {
                    Parent.Dokkaebi.StopAttack();
                }
            }

            private class DoCooldown : Work<Attack>
            {
                private float cooldown;

                protected override void OnEnter(params object[] _)
                {
                    cooldown = Parent.Cooldown;
                }

                protected override void OnUpdate()
                {
                    cooldown -= Time.deltaTime;
                    if (cooldown <= 0)
                    {
                        if (Parent.CheckPlayer(out _))
                            Parent.Parent.SetNext<Engaged>();
                        else
                            Parent.Parent.SetNext<Idle>();
                    }
                }
            }
        }
    }
}
