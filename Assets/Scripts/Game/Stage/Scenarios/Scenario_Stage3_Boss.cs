using System.Collections.Generic;
using Actors;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using Sound;

namespace Game.Stage
{
    public class Scenario_Stage3_Boss : ScenarioManager
    {
        private new FinalBossStageManager StageManager => (FinalBossStageManager)base.StageManager;

        private enum BlockName
        {
            To_Contact,
            Contact_TwinBoss_First,
            Contact_TwinBoss_Reentry,
            Battle_TwinBoss,
            Contact_FinalBoss,
            Battle_FinalBoss,
            To_Ending,
            Ending,
        }

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Contact)
                .OnUpdated<Block>(self =>
                {
                    if (IsPlayerOnGround && IsRubielClose)
                    {
                        BlockInputs();

                        if (IsFirstArrival)
                            To(BlockName.Contact_TwinBoss_First);
                        else
                            To(BlockName.Contact_TwinBoss_Reentry);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Contact_TwinBoss_First,
                dialogueTitle: "Stage3_TwinBoss_First",
                onDialogueEnd: () =>
                {
                    UnblockInputs();
                    To(BlockName.Battle_TwinBoss);
                });

            yield return new DialogueBlock(
                BlockName.Contact_TwinBoss_Reentry,
                dialogueTitle: "Stage3_TwinBoss_Reentry",
                onDialogueEnd: () =>
                {
                    UnblockInputs();
                    To(BlockName.Battle_TwinBoss);
                });

            yield return new Block(
                BlockName.Battle_TwinBoss)
                .OnEntered(() =>
                {
                    SetRubielToInvisible();
                    StageManager.Commence();
                })
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken
                        && StageManager.CurrentPhase == FinalBossStageManager.Phase.FinalBossReady
                        && IsPlayerOnGround)
                    {
                        self.ToNextToken = true;

                        BlockInputs();
                        new Timer(2f, _ => To(BlockName.Contact_FinalBoss));
                    }
                });

            yield return new DialogueBlock(
                BlockName.Contact_FinalBoss,
                dialogueTitle: "Stage3_Stage3_FinalBoss",
                onDialogueEnd: () =>
                {
                    UnblockInputs();
                    To(BlockName.Battle_FinalBoss);
                })
                .OnEntered(() => BgmPlayManager.Play(BgmName.Final_Boss));

            yield return new Block(
                BlockName.Battle_FinalBoss)
                .OnEntered(StageManager.Commence)
                .OnUpdated<Block>(self =>
                {
                    if (StageManager.CurrentPhase == FinalBossStageManager.Phase.StageCompleted
                        && IsPlayerOnGround)
                    {
                        SetRubielToVisible(true);
                        Rubiel.transform.position = new(-3.15f, -5.9f, 0);
                        Rubiel.GetComponent<TargetFollower>().IsEnabled = false;

                        StageManager.Box.gameObject.SetActive(true);
                        StageManager.Portal.gameObject.SetActive(true);

                        if (GameServices) GameServices.IsGameCleared = true;
                        To(BlockName.To_Ending);
                    }
                });

            yield return new Block(
                BlockName.To_Ending)
                .OnUpdated<Block>(self =>
                {
                    if (IsPlayerOnGround && IsRubielClose)
                    {
                        SetRubielToBig();
                        BlockInputs();
                        To(BlockName.Ending);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Ending,
                dialogueTitle: "Ending_Arrival",
                onDialogueEnd: () =>
                {
                    UnblockInputs();
                    Exit();
                });
        }
    }
}
