using System;
using System.Text;
using Actors;
using Actors.Monsters.Bosses;
using Infrastructure;
using UI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Stage
{
    public class FinalBossStageManager : StageManager, IInjectable<GameServices>
    {
        [Header("Twin Boss")]
        [SerializeField] private TwinBossManager _twinBossManager;
        [SerializeField] private GameObject _VeliaObject;
        [SerializeField] private GameObject _darkTherionObject;
        [SerializeField] private BossUI _VeliaUI;
        [SerializeField] private BossUI _darkTherionUI;
        [SerializeField, Min(0)] private float _intermissionTime = 1f;

        [Header("Final Boss")]
        [SerializeField] private GameObject _VerbelionPackage;
        [SerializeField] private GameObject _VerbelionObject;
        [SerializeField] private BossUI _VerbelionUI;
        [SerializeField] private DeferredSceneObjects _VerbelionPotions;

        [Header("State Disply")]
        [SerializeField, TextArea(3, 10)]
        private string _stateDisplay = string.Empty;
        private readonly StringBuilder _sb = new();

        public Phase CurrentPhase => _phase;

        // Internal
        private IBoss _Velia;
        private IBoss _darkTherion;
        private IBoss _Verbelion;

        public enum Phase
        {
            TwinBossReady,
            TwinBoss,
            Intermission,
            FinalBossReady,
            FinalBoss,
            StageCompleted
        }

        private GameServices _gameServices;
        private Phase _phase;


        // Front
        protected override void Awake()
        {
            AutoBindSceneMonsters = false;
            base.Awake();

            if (!_twinBossManager)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_twinBossManager)}이(가) 유효하지 않습니다.'"));

            if (!_VeliaObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VeliaObject)}이(가) 유효하지 않습니다.'"));

            if (!_VeliaObject.TryGetComponent(out _Velia))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VeliaObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_darkTherionObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionObject)}이(가) 유효하지 않습니다.'"));

            if (!_darkTherionObject.TryGetComponent(out _darkTherion))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_VeliaUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VeliaUI)}이(가) 유효하지 않습니다.'"));

            if (!_darkTherionUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionUI)}이(가) 유효하지 않습니다.'"));

            if (!_VerbelionPackage)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VerbelionPackage)}이(가) 유효하지 않습니다.'"));

            if (!_VerbelionObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VerbelionObject)}이(가) 유효하지 않습니다.'"));

            if (!_VerbelionObject.TryGetComponent(out _Verbelion))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VerbelionObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_VerbelionUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_VerbelionUI)}이(가) 유효하지 않습니다.'"));
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices)
            => _gameServices = gameServices;

        protected override void Start()
        {
            base.Start();
            StartInitial();

            _gameServices.IsStage3Reached = true;
        }

        private void RegisterBoss(IBoss boss, BossUI ui, bool createUI = true)
        {
            if (!MonsterManager.Register(boss))
                return;

            if (boss is IPlayerInitializable playerInitializable)
            {
                if (Player != null)
                    playerInitializable.InitializePlayer(Player);
                else
                    Debug.LogWarning(Ctx(
                        $"{nameof(boss)}에 {nameof(Player)}을(를) 등록하지 못했습니다. " +
                        $"해당 컴포넌트가 유효한지 확인하세요."));
            }

            if (createUI)
            {
                var vm = new MonsterVM(boss);
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui);
            }
        }

        public void Commence()
        {
            if (_phase == Phase.TwinBossReady)
                StartTwinBoss();
            else if (_phase == Phase.FinalBossReady)
                StartVerbelion();
            else if (_phase > Phase.FinalBossReady)
                Debug.LogWarning(
                    $"{nameof(_phase)}이(가) '{_phase}'이기 때문에 더 이상의 {nameof(Commence)}이(가) 불가능합니다.",
                    this);
        }

        private void StartInitial()
        {
            _twinBossManager.gameObject.SetActive(true);
            _VerbelionPackage.SetActive(false);

            _VeliaUI.SetToDisabled();
            _darkTherionUI.SetToDisabled();

            _phase = Phase.TwinBossReady;
        }

        private void StartTwinBoss()
        {
            RegisterBoss(_Velia, _VeliaUI);
            RegisterBoss(_darkTherion, _darkTherionUI);

            _VeliaUI.Enable();
            _darkTherionUI.Enable();

            _twinBossManager.Cleared += () =>
            {
                _phase = Phase.Intermission;

                _VeliaUI.Disable();
                _darkTherionUI.Disable();

                new Timer(_intermissionTime, _ =>
                {
                    _VerbelionPackage.SetActive(true);
                    _phase = Phase.FinalBossReady;
                });
            };

            _twinBossManager.Commence();
            _phase = Phase.TwinBoss;
        }

        private void StartVerbelion()
        {
            _VerbelionPotions?.Activate();

            _twinBossManager.gameObject.SetActive(false);
            RegisterBoss(_Verbelion, _VerbelionUI);

            IDisposable handle = null;
            int frameCount = 1;

            handle = Loco.Subscribe(() =>
            {
                frameCount--;
                if (frameCount > 0) return;

                _Verbelion.ConditionChanged += c =>
                {
                    if (c.Is(MonsterCondition.Dying))
                        StageCleared();
                };

                _VerbelionUI.Enable();
                _Verbelion.Commence();
                handle.Dispose();
            });

            _phase = Phase.FinalBoss;
        }

        private void StageCleared()
        {
            _phase = Phase.StageCompleted;
            Debug.Log("이겼닭! 오늘 저녁은 치킨이닭!", this);
        }


        // Debug
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                Commence();

#if UNITY_EDITOR
            _sb.Clear();
            _sb.AppendLine($"Phase: {_phase}");
            _stateDisplay = _sb.ToString();
#endif
        }

        private string Ctx(string message) => $"[{nameof(SingleBossStageManager)}] {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(FinalBossStageManager)), CanEditMultipleObjects]
        protected class FinalBossStageManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawPropertiesExcluding(serializedObject, nameof(AutoBindSceneMonsters));

                if (GUILayout.Button("Commence"))
                    ((SingleBossStageManager)target).Commence();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
