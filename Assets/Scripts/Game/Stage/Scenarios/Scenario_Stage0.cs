using System.Collections.Generic;
using Dialogue;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Game.Stage
{
    public class Scenario_Stage0 : ScenarioManager
    {
        [Header("Scenario_Stage0")]
        [SerializeField] private bool _isFirstArrival = true;

        private enum BlockName
        {
            To_Arrival,
            Arrival_First,
            Arrival_Reentry,
            To_FirstSkillAcquire,
            FirstSkillAcquire,
            To_TownPortal,
            TownPortal,
        }

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Arrival)
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken && IsPlayerOnGround && IsRubielClose)
                    {
                        self.ToNextToken = true;
                        BlockInputs();
                        SetRubielToBig(() =>
                            To(_isFirstArrival ? BlockName.Arrival_First : BlockName.Arrival_Reentry));
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_First,
                dialogueTitle: DialogueTitle.Stage0_Arrival_First,
                onDialogueEnd: () => To(BlockName.To_FirstSkillAcquire))
                .OnEntered(() => SetRubielToBig())
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall(/* () => SetRubielToInvisible() */);
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: DialogueTitle.Stage0_Arrival_Reentry,
                onDialogueEnd: () => To(BlockName.To_FirstSkillAcquire))
                .OnEntered(() => SetRubielToBig())
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall(/* () => SetRubielToInvisible() */);
                });

            yield return new Block(
                BlockName.To_FirstSkillAcquire)
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken && Input.GetKey(ProceedKey) && IsPlayerOnGround)
                    {
                        self.ToNextToken = true;
                        BlockInputs();
                        SetRubielToVisible(true, () => To(BlockName.FirstSkillAcquire));
                    }
                });

            yield return new DialogueBlock(
                BlockName.FirstSkillAcquire,
                dialogueTitle: DialogueTitle.Stage0_FirstSkillAcquire,
                onDialogueEnd: () => To(BlockName.To_TownPortal))
                .OnEntered(() => SetRubielToBig())
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall(/* () => SetRubielToInvisible() */);
                });

            yield return new Block(
                BlockName.To_TownPortal)
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken && Input.GetKey(ProceedKey) && IsPlayerOnGround)
                    {
                        self.ToNextToken = true;
                        BlockInputs();
                        SetRubielToVisible(true, () => To(BlockName.TownPortal));
                    }
                });

            yield return new DialogueBlock(
                BlockName.TownPortal,
                dialogueTitle: DialogueTitle.Stage0_TownPortal,
                onDialogueEnd: () => Exit())
                .OnEntered(() => SetRubielToBig())
                .OnExited(UnblockInputs);
        }
    }
}
