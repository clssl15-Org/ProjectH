using System.Collections.Generic;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Game.Stage
{
    public class Scenario_Stage1 : ScenarioManager
    {
        private enum BlockName
        {
            To_Arrival,
            Arrival_First,
            Arrival_Reentry,
        }

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Arrival)
                .OnEntered(() =>
                {
                    if (IsFirstArrival)
                        SetRubielToBig(instantSet: true);
                })
                .OnUpdated<Block>(self =>
                {
                    if (IsPlayerOnGround && IsRubielClose)
                    {
                        if (IsFirstArrival)
                        {
                            BlockInputs();
                            To(BlockName.Arrival_First);
                        }
                        else if (Random.Range(0, 6) != 0)
                        {
                            BlockInputs();
                            To(BlockName.Arrival_Reentry);
                        }
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_First,
                dialogueTitle: "Stage1_Arrival_First",
                onDialogueEnd: Exit)
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall(() => SetRubielToInvisible());
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: "Stage1_Arrival_Reentry",
                onDialogueEnd: Exit)
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall(() => SetRubielToInvisible());
                });
        }
    }
}
