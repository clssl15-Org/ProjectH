using System;
using System.Collections.Generic;
using Actors;
using BlackboxSystem;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Game.Stage
{
    public abstract class ScenarioManager : MonoBehaviour, IInputController
    {
        // Internal
        internal StageManager StageManager { get; private set; }
        internal ScenarioMachine Machine { get; private set; }

        [field: Tooltip("디버그용 스토리 진행 버튼")]
        [field: SerializeField] protected KeyCode ProceedKey { get; private set; } = KeyCode.Alpha0;
        [field: SerializeField] protected float TargetRubielDistance { get; private set; } = 3f;

        [Space]
        [SerializeField] private string _display;

        protected IPlayer Player => StageManager.Player;
        protected Rubiel Rubiel => StageManager.Rubiel;

        protected bool IsPlayerOnGround =>
            Player != null && Player.CurrentPlatform >= 0;
        protected bool IsRubielClose =>
            Rubiel && Vector2.Distance(Rubiel.transform.position, Player.transform.position) <= TargetRubielDistance;

        private IInputHub _inputHub;
        private bool _isInitialized = false;

        internal class ScenarioMachine : Work
        {
            internal StageManager StageManager { get; private set; }

            public ScenarioMachine(StageManager stageManager) : base("ScenarioMachine") =>
                StageManager = stageManager;
        }


        // Content
        public void Initialize(StageManager stageManager)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize, was: {_isInitialized}");

            if (_isInitialized) return;
            _isInitialized = true;

            StageManager = stageManager;

            ((IInputController)this).Initialize(stageManager.InputHub);
            Machine = new ScenarioMachine(stageManager);

            var isFisrt = true;
            foreach (var block in GetBlocks())
            {
                Machine.AddChild(block, isFisrt);
                isFisrt = false;
            }
        }
        internal abstract IEnumerable<Work> GetBlocks();

        void IInputController.Initialize(IInputHub inputHub)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("IInputHub Injected");
            _inputHub = inputHub;
        }

        protected void BlockAllInputs(bool exceptSettingsUI = true)
        {
            BlackboxHandle.Of(this).Exert(_inputHub,
                $"Block All, exceptSettingsUI: {exceptSettingsUI}");

            if (exceptSettingsUI) _inputHub.BlockExcept(StageManager.SettingsUI);
            else _inputHub.BlockAll();
        }
        protected void UnblockAllInputs()
        {
            BlackboxHandle.Of(this).Exert(_inputHub, "Unblock All");
            _inputHub.UnblockAll();
        }

        protected virtual void Start() => Machine?.Enter();
        private void Update()
        {
            Machine?.Update();

# if UNITY_EDITOR
            if (Machine != null)
            {
                if (Machine.TryGetCurrentChild(out var child))
                    _display = $"Current: {child.Name}";
                else
                    _display = "Current is null";
            }
#endif
        }

        protected void SetRubielToBig(Action callback = null) => StageManager.Rubiel.ToBig(callback);
        protected void SetRubielToSmall(Action callback = null) => StageManager.Rubiel.ToSmall(callback);
        protected void SetRubielToVisible(Action callback = null)
        {
            if (Rubiel.IsTotallyInvisible)
                TransferRubielNearToPlayer();

            StageManager.Rubiel.ToVisible(callback);
        }
        protected void SetRubielToInvisible(Action callback = null) => StageManager.Rubiel.ToInvisible(callback);
        protected void TransferRubielNearToPlayer()
        {
            Rubiel.Teleport(
                position: (Vector2)Player.transform.position + Player.Direction.ToVector2() * TargetRubielDistance, 
                lookRight: Player.Direction.Flip().ToVector2().x > 0);
        }

        public void To(object blockName)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"To: {blockName}");
            Machine.SetNext(blockName);
        }
        public void Exit()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop");
            Machine.Exit();
        }

        private void OnDestroy()
        {
            BlackboxHandle.Of(this).WriteScope("Destroy");

            if (Machine != null)
            {
                Machine.Dispose();
                Machine = null;
            }
        }
    }
}
