using System.Collections.Generic;
using Infrastructure.StateMachines.Fsm;

namespace Game.Stage
{
    public class Scenario_Stage2_Boss : ScenarioManager
    {
        private new SingleBossStageManager StageManager => (SingleBossStageManager)base.StageManager;

        private enum BlockName
        {
            To_Contact,
            Contact,
            Battle,
            Passed,
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
                dialogueTitle: "Stage2_Boss_Contact",
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
                        SetRubielToVisible(RubielVisibilityMode.TeleportNearToPlayer, () => To(BlockName.Passed));
                    }
                });

            yield return new DialogueBlock(
                BlockName.Passed,
                dialogueTitle: "Stage2_Boss_Passed",
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
