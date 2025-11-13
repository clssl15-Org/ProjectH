using UnityEngine;
using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class OnValidPlatform : Work<Alive>
        {
            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            public OnValidPlatform()
            {
                AddChild(new Idle(), true);
                AddChild(new Engaged());
                AddChild(new Attack());
            }

            protected override void OnUpdate()
            {
                if (!Dokkaebi.PlatformDetector.CheckPlatform(
                    Direction.Center, Dokkaebi.BelongingPlatform, out _))
                {
                    Dokkaebi.BelongingPlatform = -1;
                    Parent.SetNext<SetPlatform>();

                    return;
                }
            }

            public bool CheckPlayer(out GameObject player)
            {
                player = Dokkaebi.DetectedPlayer;

                if (!player)
                    return false;

                var playerPlatform = player.GetComponent<TestPlayer>().CurrentPlatform;
                return playerPlatform >= 0 && playerPlatform == Dokkaebi.BelongingPlatform;
            }
        }
    }
}
