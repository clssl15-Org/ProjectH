using System.Collections.Generic;
using Infrastructure.StateMachines.Fsm;

namespace Game.Stage
{
    public class Scenario_Stage1_Boss : ScenarioManager
    {
        private new SingleBossStageManager StageManager => (SingleBossStageManager)base.StageManager;

        private enum BlockName
        {
            To_Contact,
            Contact,
            Battle,
            Passed_1,
            Passed_2,
        }

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Contact)
                .OnEntered(() =>
                {
                    if (!IsFirstArrival)
                        Rubiel.SetToInvisible();
                })
                .OnUpdated<Block>(self =>
                {
                    if (IsPlayerOnGround && IsRubielClose)
                    {
                        if (IsFirstArrival)
                        {
                            BlockInputs();
                            To(BlockName.Contact);
                        }
                        else
                            To(BlockName.Battle);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Contact,
                dialogueTitle: "Stage1_Boss_Contact",
                onDialogueEnd: () =>
                {
                    UnblockInputs();
                    To(BlockName.Battle);
                });

            yield return new Block(
                BlockName.Battle)
                .OnEntered(() =>
                {
                    SetRubielToInvisible();
                    StageManager.Commence();
                })
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken && StageManager.IsCleared && IsPlayerOnGround)
                    {
                        self.ToNextToken = true;

                        BlockInputs();
                        SetRubielToVisible(RubielVisibilityMode.TeleportNearToPlayer, () => To(BlockName.Passed_1));
                    }
                });

            yield return new DialogueBlock(
                BlockName.Passed_1,
                dialogueTitle: "Stage1_Boss_Passed_1",
                onDialogueEnd: () => To(BlockName.Passed_2));

            yield return new DialogueBlock(
                BlockName.Passed_2,
                dialogueTitle: "Stage1_Boss_Passed_2",
                onDialogueEnd: () =>
                {
                    UnblockInputs();

                    StageManager.Box.gameObject.SetActive(true);
                    StageManager.Portal.gameObject.SetActive(true);

                    Exit();
                });
        }
    }
}
