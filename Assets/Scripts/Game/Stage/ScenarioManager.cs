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
        [SerializeField] private string _arrivalKey;

        protected bool IsFirstArrival => !_overrideFirstArrival.Resolve(false)
            ? _isFirstArrivalForCurrentRun : _isFirstArrival;

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
        private bool _isFirstArrivalResolved = false;
        private bool _isFirstArrivalForCurrentRun = true;

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

        void IInjectable<GameServices>.Inject(GameServices gameServices)
            => GameServices = gameServices;

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

        protected virtual void Start()
        {
            ResolveFirstArrival();
            Machine?.Enter();
        }
        protected virtual void Update()
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

        public enum RubielVisibilityMode
        {
            KeepPosition,
            NearToPlayer,
            TeleportNearToPlayer,
        }
        protected void SetRubielToVisible(RubielVisibilityMode visibilityMode = RubielVisibilityMode.KeepPosition, Action callback = null)
        {
            _rubielVisibleHandle?.Dispose();

            bool isClose = IsRubielClose;
            bool isVisible = Rubiel.IsTotallyVisible;

            if (visibilityMode == RubielVisibilityMode.TeleportNearToPlayer)
            {
                TransferRubielNearToPlayer(3f);
                isClose = true;
            }
            else if (visibilityMode == RubielVisibilityMode.NearToPlayer
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
        protected void TransferRubielNearToPlayer(float distance = -1f)
        {
            if (distance < 0f)
                distance = TargetRubielDistance;

            Direction direction = Player.Direction;
            Vector2 position = GetRubielNearPlayerPosition(direction, distance);

            if (!IsInMainCameraView(position))
            {
                direction = direction.Flip();
                position = GetRubielNearPlayerPosition(direction, distance);
            }

            Rubiel.Teleport(
                position: position,
                lookRight: direction.Flip().ToVector2().x > 0);
        }

        private Vector2 GetRubielNearPlayerPosition(Direction direction, float distance) =>
            (Vector2)Player.transform.position + direction.ToVector2() * distance;

        private bool IsInMainCameraView(Vector2 position)
        {
            Camera mainCamera = Camera.main;
            if (!mainCamera)
                return true;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
            return viewportPoint.z >= 0f
                && viewportPoint.x >= 0f && viewportPoint.x <= 1f
                && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
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

        private void ResolveFirstArrival()
        {
            if (_isFirstArrivalResolved)
                return;

            _isFirstArrivalResolved = true;

            if (_overrideFirstArrival.Resolve(false))
            {
                _isFirstArrivalForCurrentRun = _isFirstArrival;
                return;
            }

            if (!GameServices)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    $"[{nameof(ScenarioManager)}] {nameof(GameServices)}가 주입되지 않아 최초도달로 처리합니다."),
                    this);
                _isFirstArrivalForCurrentRun = true;
                return;
            }

            _isFirstArrivalForCurrentRun = GameServices.ConsumeFirstScenarioArrival(GetArrivalKey(), this);
        }

        private string GetArrivalKey()
        {
            if (!string.IsNullOrWhiteSpace(_arrivalKey))
                return _arrivalKey.Trim();

            var scene = gameObject.scene;
            if (scene.IsValid() && !string.IsNullOrWhiteSpace(scene.name))
                return scene.name;

            return GetType().FullName;
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
