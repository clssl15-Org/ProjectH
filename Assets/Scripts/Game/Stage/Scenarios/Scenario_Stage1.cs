using System.Collections.Generic;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using Sound;
using UnityEngine;

namespace Game.Stage
{
    public class Scenario_Stage1 : ScenarioManager
    {
        [SerializeField] private bool _overrideCleared;
        [SerializeField] private bool _isCleared;

        private enum BlockName
        {
            To_Arrival,
            Arrival_First,
            Arrival_Reentry,
            Ending,
        }

        protected override void Start()
        {
            if (_overrideCleared && _isCleared.Resolve(false))
                GameServices.IsGameCleared = true;

            base.Start();

            if (GameServices.IsGameCleared)
                BgmPlayManager.Stop();
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
                        if (GameServices.IsGameCleared)
                        {
                            BlockInputs();
                            To(BlockName.Ending);
                        }
                        else if (IsFirstArrival)
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
                    SetRubielToSmall();
                });

            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: "Stage1_Arrival_Reentry",
                onDialogueEnd: Exit)
                .OnExited(() =>
                {
                    UnblockInputs();
                    SetRubielToSmall();
                });

            yield return new DialogueBlock(
                BlockName.Ending,
                dialogueTitle: "Ending_Arrival",
                onDialogueEnd: () =>
                {
                    StageManager.DarkscreenUI.CloseScreen(
                        () => GameServices.ChangeScene("EndingScene"));

                    Exit();
                });
        }
    }
}
