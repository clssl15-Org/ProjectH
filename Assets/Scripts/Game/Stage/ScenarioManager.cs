using System;
using System.Collections.Generic;
using Actors;
using BlackboxSystem;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Game.Stage
{
    public abstract class ScenarioManager : MonoBehaviour,
        IInputLayerSubject,
        IInputLayerController,
        IInjectable<GameServices>
    {
        // Internal
        internal StageManager StageManager { get; private set; }
        internal ScenarioMachine Machine { get; private set; }

        [SerializeField] private bool _overrideFirstArrival;
        [SerializeField] private bool _isFirstArrival = true;

        protected bool IsFirstArrival => !_overrideFirstArrival.Resolve(false)
            ? !GameServices.PlayerHasDied : _isFirstArrival;

        [Tooltip("디버그용 스토리 진행 버튼")]
        [SerializeField] private KeyCode _proceedKey = KeyCode.Alpha0;
        [field: SerializeField] protected float TargetRubielDistance { get; private set; } = 3f;

        [Space]
        [SerializeField] private string _display;

        protected IPlayer Player => StageManager.Player;
        protected Rubiel Rubiel => StageManager.Rubiel;
        protected KeyCode ProceedKey => _proceedKey.Resolve();
        protected GameServices GameServices { get; private set; }

        public bool AllowInput { get;set; } = true;
        bool IInputLayerSubject.IsTrigger => false;

        public event Action Destroying;

        protected bool IsPlayerOnGround =>
            Player != null && Player.CurrentPlatform >= 0;
        protected bool IsRubielClose =>
            Rubiel && Vector2.Distance(Rubiel.transform.position, Player.transform.position) <= TargetRubielDistance;

        private IInputHub _inputHub;
        private IDisposable _rubielVisibleHandle;
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
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize, wasInitialized: {_isInitialized}");

            if (_isInitialized) return;
            _isInitialized = true;

            StageManager = stageManager;

            ((IInputLayerController)this).Initialize(stageManager.InputHub);
            Machine = new ScenarioMachine(stageManager);

            var isFisrt = true;
            foreach (var block in GetBlocks())
            {
                Machine.AddChild(block, isFisrt);
                isFisrt = false;
            }
        }
        internal abstract IEnumerable<Work> GetBlocks();

        void IInjectable<GameServices>.Inject(GameServices gameServices) =>
            GameServices = gameServices;

        void IInputLayerController.Initialize(IInputHub inputHub)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("InputHub Injected");
            _inputHub = inputHub;
        }

        protected void BlockInputs()
        {
            BlackboxHandle.Of(this).Exert(_inputHub, "Block");

            if (StageManager.PlayerUI != null)
                _inputHub.AddAfter(StageManager.PlayerUI, this);
            else if (StageManager.Player != null)
                _inputHub.AddAfter(StageManager.Player, this);
            else
                _inputHub.Add(this);

            _inputHub.AddAfter(this, StageManager.DialogueManager);
        }
        protected void UnblockInputs()
        {
            BlackboxHandle.Of(this).Exert(_inputHub, "Unblock");

            _inputHub.Remove(StageManager.DialogueManager);
            _inputHub.Remove(this);
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

        protected void SetRubielToBig(Action callback = null, bool instantSet = false)
        {
            _rubielVisibleHandle?.Dispose();

            if (instantSet)
            {
                Rubiel.SetToBig();
                callback?.Invoke();
            }
            else
                Rubiel.ToBig(callback);
        }
        protected void SetRubielToSmall(Action callback = null, bool instantSet = false)
        {
            _rubielVisibleHandle?.Dispose();

            if (instantSet)
            {
                Rubiel.SetToSmall();
                callback?.Invoke();
            }
            else
                Rubiel.ToSmall(callback);
        }
        protected void SetRubielToVisible(bool shouldNearToPlayer, Action callback = null)
        {
            _rubielVisibleHandle?.Dispose();

            bool isClose = IsRubielClose;
            bool isVisible = Rubiel.IsTotallyVisible;

            if (shouldNearToPlayer
                && !IsRubielClose
                && Rubiel.IsTotallyInvisible)
            {
                TransferRubielNearToPlayer();
                isClose = true;
            }

            if (!isVisible)
                Rubiel.ToVisible();

            if (isClose && isVisible)
                callback?.Invoke();
            else
            {
                IDisposable handle = null;
                _rubielVisibleHandle = Loco.Subscribe(() =>
                {
                    if (!Rubiel || handle != _rubielVisibleHandle)
                    {
                        handle.Dispose();
                        return;
                    }

                    if (IsRubielClose && Rubiel.IsTotallyVisible)
                    {
                        handle.Dispose();
                        _rubielVisibleHandle = null;

                        callback?.Invoke();
                    }
                });

                handle = _rubielVisibleHandle;
            }
        }
        protected void SetRubielToInvisible(Action callback = null)
        {
            _rubielVisibleHandle?.Dispose();
            Rubiel.ToInvisible(callback);
        }
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Exit");
            Machine.Exit();
        }

        private void OnDestroy()
        {
            BlackboxHandle.Of(this).WriteScope("Destroy");

            _rubielVisibleHandle?.Dispose();
            Destroying?.Invoke();

            if (Machine != null)
            {
                Machine.Dispose();
                Machine = null;
            }
        }
    }
}
