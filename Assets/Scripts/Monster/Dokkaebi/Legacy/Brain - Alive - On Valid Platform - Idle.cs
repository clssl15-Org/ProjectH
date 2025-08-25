using UnityEngine;
using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class Idle : Work<OnValidPlatform>
        {
            // Front
            public float MinRestTime { get; set; } = 0.5f;
            public float MaxRestTime { get; set; } = 2f;
            public float MinPatrolTime { get; set; } = 1f;
            public float MaxPatrolTime { get; set; } = 10f;

            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            public Idle()
            {
                AddChild(new Rest());
                AddChild(new Patrol());
            }

            /// <summary>
            /// startWithIdle이 참이면 첫 상태로 Rest를 선택합니다.
            /// </summary>
            protected override void OnEnter(params object[] startWithIdle)
            {
                if (startWithIdle.Length > 0 && (bool)startWithIdle[0])
                    SetNext<Rest>();
                else
                {
                    if (Random.Range(0, 3) == 0)
                        SetNext<Rest>();
                    else
                        SetNext<Patrol>();
                }
            }

            protected override void OnUpdate()
            {
                if (Parent.CheckPlayer(out _))
                    Parent.SetNext<Engaged>();
            }


            // Substates
            private class Rest : Work<Idle>
            {
                private float remainingTime = 0;

                protected override void OnEnter(params object[] _)
                {
                    remainingTime = Random.Range(Parent.MinRestTime, Parent.MaxRestTime);

                    Parent.Dokkaebi.Rigidbody.velocity = new Vector2
                    {
                        x = 0,
                        y = Parent.Dokkaebi.Rigidbody.velocity.y,
                    };
                }

                protected override void OnUpdate()
                {
                    remainingTime -= Time.deltaTime;

                    if (remainingTime <= 0)
                        Parent.SetNext<Patrol>();
                }
            }

            private class Patrol : Work<Idle>
            {
                private Dokkaebi Dokkaebi => Parent.Dokkaebi;
                private float remainingTime = 0;


                protected override void OnEnter(params object[] _)
                {
                    remainingTime = Random.Range(Parent.MinPatrolTime, Parent.MaxPatrolTime);

                    if (Dokkaebi.Direction != Direction.Left && Dokkaebi.Direction != Direction.Right)
                        Dokkaebi.Direction = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;
                }

                protected override void OnUpdate()
                {
                    remainingTime -= Time.deltaTime;

                    if (remainingTime <= 0)
                        Parent.SetNext<Rest>();


                    if (!Dokkaebi.TryMove())
                    {
                        Dokkaebi.Direction = (Dokkaebi.Direction == Direction.Left)
                            ? Direction.Right
                            : Direction.Left;
                    }
                }

                protected override void OnExit()
                {
                    Dokkaebi.Rigidbody.velocity = new Vector2
                    {
                        x = 0,
                        y = Dokkaebi.Rigidbody.velocity.y,
                    };
                }
            }
        }
    }
}
