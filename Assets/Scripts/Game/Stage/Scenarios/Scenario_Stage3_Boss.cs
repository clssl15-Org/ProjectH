using System.Collections.Generic;
using Actors;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using Sound;
using UnityEngine;
using UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Stage
{
    public class Scenario_Stage3_Boss : ScenarioManager
    {
        private const string FinalBossContactArrivalKey = "Stage3Boss:FinalBossContact";
        private const string EndingArrivalKey = "Stage3Boss:Ending";

        [Space]
        [SerializeField] private string NextSceneName = "EndingScene";
        [SerializeField] private KeyCode ToEndingKey;

        private new FinalBossStageManager StageManager => (FinalBossStageManager)base.StageManager;
        private readonly object _finalBossTransitionInvincibleSource = new();

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

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(ToEndingKey.Resolve()))
                ToEnding();
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

                        SetFinalBossTransitionInvincible(true);
                        BlockInputs();
                        new Timer(2f, _ =>
                        {
                            if (ConsumePostBossArrival(FinalBossContactArrivalKey))
                                To(BlockName.Contact_FinalBoss);
                            else
                                SkipFinalBossContact();
                        });
                    }
                });

            yield return new DialogueBlock(
                BlockName.Contact_FinalBoss,
                dialogueTitle: "Stage3_Stage3_FinalBoss",
                onDialogueEnd: () =>
                {
                    ToFinalBossBattle();
                })
                .OnEntered(() => BgmPlayManager.Play(BgmName.Final_Boss));

            yield return new Block(
                BlockName.Battle_FinalBoss)
                .OnEntered(() =>
                {
                    SetFinalBossTransitionInvincible(false);
                    StageManager.Commence();
                })
                .OnUpdated<Block>(self =>
                {
                    if (!self.ToNextToken
                        && StageManager.CurrentPhase == FinalBossStageManager.Phase.StageCompleted
                        && IsPlayerOnGround)
                    {
                        self.ToNextToken = true;
                        new Timer(3f, succeeded =>
                        {
                            if (!succeeded)
                                return;

                            SetRubielToVisible(RubielVisibilityMode.TeleportNearToPlayer);
                            Rubiel.GetComponent<TargetFollower>().IsEnabled = false;

                            BgmPlayManager.Stop();

                            SetGameCleared();
                            if (ConsumePostBossArrival(EndingArrivalKey))
                                To(BlockName.To_Ending);
                            else
                                ChangeToEndingScene();
                        });
                    }
                });

            yield return new Block(
                BlockName.To_Ending)
                .OnUpdated<Block>(self =>
                {
                    if (IsPlayerOnGround)
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
                    ChangeToEndingScene();
                });
        }

        private bool ConsumePostBossArrival(string arrivalKey)
        {
            return !GameServices || GameServices.ConsumeFirstScenarioArrival(arrivalKey, this);
        }

        private void SkipFinalBossContact()
        {
            BgmPlayManager.Play(BgmName.Final_Boss);
            ToFinalBossBattle();
        }

        private void ToFinalBossBattle()
        {
            UnblockInputs();
            To(BlockName.Battle_FinalBoss);
        }

        private void ChangeToEndingScene()
        {
            SetGameCleared();

            if (GameServices)
            {
                FindAnyObjectByType<DarkscreenUI>(FindObjectsInactive.Include).CloseScreen(() =>
                    GameServices.ChangeScene(NextSceneName, this));
            }

            Exit();
        }

        private void SetGameCleared()
        {
            if (GameServices)
                GameServices.IsGameCleared = true;
        }

        private void ToEnding()
        {
            if (!DebugTools.IsDebugMode)
                return;

            SetFinalBossTransitionInvincible(false);
            BlockInputs();
            To(BlockName.Ending);
        }

        private void SetFinalBossTransitionInvincible(bool invincible)
        {
            StageManager?.Player?.SetInvincibleOverride(_finalBossTransitionInvincibleSource, invincible);
        }

        private void OnDisable()
        {
            SetFinalBossTransitionInvincible(false);
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Scenario_Stage3_Boss))]
        protected class Scenario_Stage3_BossEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("To Ending"))
                    ((Scenario_Stage3_Boss)target).ToEnding();
            }
        }
#endif
    }
}
