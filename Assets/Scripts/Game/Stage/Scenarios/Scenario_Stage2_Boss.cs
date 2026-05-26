using System.Collections.Generic;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Game.Stage
{
    public class Scenario_Stage2_Boss : ScenarioManager
    {
        private const string PassedArrivalKey = "Stage2Boss:Passed";

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

                        if (ConsumePassedArrival())
                        {
                            BlockInputs();
                            SetRubielToVisible(RubielVisibilityMode.TeleportNearToPlayer, () => To(BlockName.Passed));
                        }
                        else
                        {
                            ActivateClearObjects();

                            Exit();
                        }
                    }
                });

            yield return new DialogueBlock(
                BlockName.Passed,
                dialogueTitle: "Stage2_Boss_Passed",
                onDialogueEnd: () =>
                {
                    UnblockInputs();

                    ActivateClearObjects();

                    Exit();
                });
        }

        private void ActivateClearObjects()
        {
            GameObject boxObject = StageManager.Box.gameObject;
            GameObject portalObject = StageManager.Portal.gameObject;
            GameObject clearObjectsRoot = FindCommonRoot(boxObject, portalObject);

            if (clearObjectsRoot)
                ClearObjectsActivator.ActivateRootAndChildren(clearObjectsRoot);
            else
            {
                boxObject.SetActive(true);
                portalObject.SetActive(true);
            }
        }

        private bool ConsumePassedArrival()
        {
            return !GameServices || GameServices.ConsumeFirstScenarioArrival(PassedArrivalKey, this);
        }

        private static GameObject FindCommonRoot(GameObject first, GameObject second)
        {
            Transform candidate = first.transform;
            while (candidate)
            {
                if (second.transform.IsChildOf(candidate))
                    return candidate.gameObject;

                candidate = candidate.parent;
            }

            return first.transform.parent ? first.transform.parent.gameObject : first;
        }
    }
}
