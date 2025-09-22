using System;
using MonsterActions;
using UnityEngine;

public partial class Ghost
{
    internal class GhostExplosiveAttackAction : MonsterActionState
    {
        // Front
        public float TeleportDistance { get; set; } = 1.5f;

        // Internal
        private Vector3 originalPosition;


        // Content
        public GhostExplosiveAttackAction() : base("Attack_2")
        {
            
        }

        protected override void Initialize()
        {
            AddChild("Approaching", new MonsterActionState("Teleportation", "Teleportation_In"), true);
            AddChild("Approached", new MonsterActionState("Teleportation", "Teleportation_Out")
                .SetEnteredAction(() =>
                {
                    originalPosition = Owner.transform.position;
                    Owner.transform.position = Owner.DetectedPlayer.transform.position + TeleportDistance *
                        (Owner.Direction == Direction.Left ? Vector3.right : Vector3.left);
                }));

            AddChild("Attack", new MonsterActionState("Attack_2"));

            AddChild("Retreating", new MonsterActionState("Teleportation", "Teleportation_In"));
            AddChild("Retreated", new MonsterActionState("Teleportation", "Teleportation_Out")
                .SetEnteredAction(() => Owner.transform.position = originalPosition));
        }
        
        protected override void OnEnter(params object[] inputs)
        {
            SetInputs(inputs);
            isCompleted = false;

            var chain = new[]
            {
                "Approaching",
                "Approached",
                "Attack",
                "Retreating",
                "Retreated"
            };

            var it = chain.GetEnumerator();

            void Advance(ActionResult result)
            {
                if (!result)
                {
                    if (result.Result != ActionResult.ResultType.Interrupted)
                        UnityEngine.Debug.LogWarning(
                            Owner.Ctx($"애니메이션 재생에 실패했습니다.\n{result.ToString()}"));

                    Exit();
                    return;
                }

                if (!it.MoveNext())
                {
                    Complete();
                    return;
                }

                SetNextWith(it.Current, (Action<ActionResult>)Advance, null, 0f);
            }

            Advance(new ActionResult(ActionResult.ResultType.Success));
        }
    }
}
