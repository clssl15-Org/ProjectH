using UnityEngine;
using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
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

            protected override void Start(params object[] _)
            {
                if (Random.Range(0, 3) == 0)
                    SetNext(typeof(Rest));
                else
                    SetNext(typeof(Patrol));
            }


            // Substates
            private class Rest : Work<Idle>
            {
                private float remainingTime = 0;

                protected override void Start(params object[] _)
                {
                    remainingTime = Random.Range(Parent.MinRestTime, Parent.MaxRestTime);

                    Parent.Dokkaebi.Rigidbody.velocity = new Vector2
                    {
                        x = 0,
                        y = Parent.Dokkaebi.Rigidbody.velocity.y,
                    };
                }

                protected override void Update()
                {
                    remainingTime -= Time.deltaTime;

                    if (remainingTime <= 0)
                        Parent.SetNext(typeof(Patrol));
                }
            }

            private class Patrol : Work<Idle>
            {
                private Dokkaebi Dokkaebi => Parent.Dokkaebi;
                private float remainingTime = 0;


                protected override void Start(params object[] _)
                {
                    remainingTime = Random.Range(Parent.MinPatrolTime, Parent.MaxPatrolTime);
                }

                protected override void Update()
                {
                    remainingTime -= Time.deltaTime;

                    if (remainingTime <= 0)
                        Parent.SetNext(typeof(Rest));

                    if (Dokkaebi.Direction == Direction.Left)
                    {
                        if (!Dokkaebi.PlatformDetector.CheckPlatform(Direction.Left, Dokkaebi.BelongingPlatform, out _))
                        {
                            Dokkaebi.Direction = Direction.Right;
                            return;
                        }

                        Dokkaebi.Rigidbody.velocity = new Vector2
                        {
                            x = -Dokkaebi.MoveSpeed,
                            y = Dokkaebi.Rigidbody.velocity.y,
                        };
                    }
                    else if (Dokkaebi.Direction == Direction.Right)
                    {
                        if (!Dokkaebi.PlatformDetector.CheckPlatform(Direction.Right, Dokkaebi.BelongingPlatform, out _))
                        {
                            Dokkaebi.Direction = Direction.Left;
                            return;
                        }

                        Dokkaebi.Rigidbody.velocity = new Vector2
                        {
                            x = Dokkaebi.MoveSpeed,
                            y = Dokkaebi.Rigidbody.velocity.y,
                        };
                    }
                    else
                        throw new System.InvalidOperationException(
                            $"Left 혹은 Right를 향하고 있지 않은 도깨비를 이동할 수 없습니다. 현재 방향: {Dokkaebi.Direction}");
                }

                protected override void Stop()
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
