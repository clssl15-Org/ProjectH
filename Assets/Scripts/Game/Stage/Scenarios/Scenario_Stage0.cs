using System.Collections.Generic;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using Sound;
using UnityEngine;

namespace Game.Stage
{
    public class Scenario_Stage0 : ScenarioManager, IInjectable<SfxPlayManager>
    {
        private enum BlockName
        {
            To_Arrival,
            Arrival_First_1,
            Arrival_First_InputName,
            Arrival_First_2,
            Arrival_Reentry,
            Arrival_Reentry_PendingForNameChange,
            Arrival_Reentry_ChangeName_1,
            Arrival_Reentry_ChangeName_InputName,
            Arrival_Reentry_ChangeName_2,
        }

        private SfxPlayManager _sfxPlayManager;

        void IInjectable<SfxPlayManager>.Inject(SfxPlayManager sfxPlayManager) =>
            _sfxPlayManager = sfxPlayManager;

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Arrival)
                .OnEntered(() =>
                {
                    if (!IsFirstArrival)
                        _sfxPlayManager.Play(SfxName.Revive);

                    SetRubielToBig(instantSet: true);
                })
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken && IsPlayerOnGround && IsRubielClose)
                    {
                        self.ToNextToken = true;

                        if (IsFirstArrival)
                        {
                            BlockInputs();
                            To(BlockName.Arrival_First_1);
                        }
                        else
                        {
                            if (Random.Range(0, 8) != 0)
                            {
                                BlockInputs();
                                To(BlockName.Arrival_Reentry);
                            }
                            else
                                To(BlockName.Arrival_Reentry_PendingForNameChange);
                        }
                    }
                });


            #region First Arrival
            yield return new DialogueBlock(
                BlockName.Arrival_First_1,
                dialogueTitle: "Stage0_Arrival_First_1",
                onDialogueEnd: () => To(BlockName.Arrival_First_InputName))
                { CloseDialogue = false }
                .OnEntered(() => SetRubielToBig());

            yield return new Block(
                BlockName.Arrival_First_InputName)
                .OnEntered(() => StageManager.DialogueUI.IsInputMode = true)
                .OnUpdated<Block>(self =>
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        GameServices.SetPlayerName(StageManager.DialogueUI.InputText.Trim());
                        StageManager.DialogueUI.IsInputMode = false;

                        To(BlockName.Arrival_First_2);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_First_2,
                dialogueTitle: "Stage0_Arrival_First_2",
                onDialogueEnd: Exit)
                { OpenDialogue = false }
                .OnExited(UnblockInputs);
            #endregion


            #region Reentry
            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: "Stage0_Arrival_Reentry",
                onDialogueEnd: () => To(BlockName.Arrival_Reentry_PendingForNameChange))
                .OnEntered(() => SetRubielToBig())
                .OnExited(UnblockInputs);

            yield return new Block(
                BlockName.Arrival_Reentry_PendingForNameChange)
                .OnUpdated<Block>(self =>
                {
                    if (IsRubielClose && Input.GetKeyDown(KeyCode.F))
                    {
                        BlockInputs();
                        To(BlockName.Arrival_Reentry_ChangeName_1);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry_ChangeName_1,
                dialogueTitle: "Satge0_Arrival_Reentry_ChangeName_1",
                onDialogueEnd: () => To(BlockName.Arrival_Reentry_ChangeName_InputName))
                { CloseDialogue = false }
                .OnEntered(() => SetRubielToBig());

            yield return new Block(
                BlockName.Arrival_Reentry_ChangeName_InputName)
                .OnEntered(() => StageManager.DialogueUI.IsInputMode = true)
                .OnUpdated<Block>(self =>
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        GameServices.SetPlayerName(StageManager.DialogueUI.InputText.Trim());
                        StageManager.DialogueUI.IsInputMode = false;

                        To(BlockName.Arrival_Reentry_ChangeName_2);
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry_ChangeName_2,
                dialogueTitle: "Satge0_Arrival_Reentry_ChangeName_2",
                onDialogueEnd: () => To(BlockName.Arrival_Reentry_PendingForNameChange))
                { OpenDialogue = false }
                .OnExited(UnblockInputs);
            #endregion
        }
    }
}
