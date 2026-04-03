using System.Collections.Generic;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using Sound;
using UnityEngine;
using World;

namespace Game.Stage
{
    public class Scenario_Stage0 : ScenarioManager, IInjectable<SfxPlayManager>
    {
        [SerializeField] private PortalDetector _portalDetector;

        private enum BlockName
        {
            To_Arrival,
            Arrival_First_1,
            Arrival_First_InputName,
            Arrival_First_2,
            Arrival_Idle,
            Arrival_Reentry,
            Arrival_Reentry_ChangeName_1,
            Arrival_Reentry_ChangeName_InputName,
            Arrival_Reentry_ChangeName_2,
            FirstSkillAcquire_Dialogue,
            FirstSkillAcquire_Guide,
            TownPortal,
        }

        private SfxPlayManager _sfxPlayManager;
        private bool _nameChanged;
        private bool _skillAcquired;
        private bool _portalReached;

        void IInjectable<SfxPlayManager>.Inject(SfxPlayManager sfxPlayManager) =>
            _sfxPlayManager = sfxPlayManager;

        protected override void Start()
        {
            base.Start();

            if (IsFirstArrival)
            {
                // 미리 상자 여는 것 방지
                StageManager.Box.IsLocked = true;

                // 스킬 얻을 때 이동
                StageManager.RelicAcquisitionUI.Disabling += () =>
                {
                    if (_nameChanged)
                        To(BlockName.FirstSkillAcquire_Dialogue);
                };

                // 포탈 대화
                _portalDetector.PlayerDetected += () =>
                {
                    if (_skillAcquired)
                    {
                        if (_portalReached) return;
                        _portalReached = true;

                        To(BlockName.TownPortal);
                    }
                };
            }
        }

        internal override IEnumerable<Work> GetBlocks()
        {
            yield return new Block(
                BlockName.To_Arrival)
                .OnEntered(() =>
                {
                    if (IsFirstArrival)
                        SetRubielToBig(instantSet: true);
                    else
                        _sfxPlayManager.Play(SfxName.Revive);
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
                                To(BlockName.Arrival_Idle);
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
                onDialogueEnd: () => To(BlockName.Arrival_Idle))
                { OpenDialogue = false }
                .OnExited(() =>
                {
                    _nameChanged = true;
                    StageManager.Box.IsLocked = false;

                    UnblockInputs();
                    SetRubielToSmall();
                });
            #endregion


            #region Reentry
            yield return new DialogueBlock(
                BlockName.Arrival_Reentry,
                dialogueTitle: "Stage0_Arrival_Reentry",
                onDialogueEnd: () => To(BlockName.Arrival_Idle))
                .OnExited(UnblockInputs);

            yield return new Block(
                BlockName.Arrival_Idle)
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
                onDialogueEnd: () => To(BlockName.Arrival_Idle))
                { OpenDialogue = false }
                .OnExited(() =>
                {
                    _nameChanged = true;
                    UnblockInputs();
                });
            #endregion


            yield return new DialogueBlock(
                BlockName.FirstSkillAcquire_Dialogue,
                dialogueTitle: "Stage0_FirstSkillAcquire",
                onDialogueEnd: () => To(BlockName.FirstSkillAcquire_Guide))
                .OnEntered(BlockInputs)
                .OnExited(() =>
                {
                    _skillAcquired = true;
                    UnblockInputs();
                });

            yield return new Block(
                BlockName.FirstSkillAcquire_Guide)
                .OnEntered(() => StageManager.GuideAndWorldRecordsUI.Open())
                .OnExited(() =>
                {
                    _skillAcquired = true;

                    UnblockInputs();
                    To(BlockName.Arrival_Idle);
                });

            yield return new DialogueBlock(
                BlockName.TownPortal,
                dialogueTitle: "Stage0_TownPortal",
                onDialogueEnd: Exit)
                .OnEntered(BlockInputs)
                .OnExited(UnblockInputs);
        }
    }
}
