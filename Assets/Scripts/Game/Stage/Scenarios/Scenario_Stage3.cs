using System.Collections.Generic;
using Infrastructure.StateMachines.Fsm;

namespace Game.Stage
{
    public class Scenario_Stage3 : ScenarioManager
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
                    if (!self.ToNextToken && IsPlayerOnGround && IsRubielClose)
                    {
                        self.ToNextToken = true;

                        if (IsFirstArrival)
                        {
                            BlockInputs();
                            To(BlockName.Arrival_First);
                        }
                        else if (UnityEngine.Random.Range(0, 6) != 0)
                        {
                            BlockInputs();
                            To(BlockName.Arrival_Reentry);
                        }
                    }
                });

            yield return new DialogueBlock(
                BlockName.Arrival_First,
                dialogueTitle: "Stage3_Arrival_First",
                onDialogueEnd: Exit)
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall();
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: "Stage3_Arrival_Reentry",
                onDialogueEnd: Exit)
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall();
                });
        }
    }
}
