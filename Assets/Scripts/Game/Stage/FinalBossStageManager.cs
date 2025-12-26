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
    public class FinalBossStageManager : StageManager
    {
        [Header("Player")]
        [SerializeField] private GameObject _playerObject;

        [Header("Twin Boss")]
        [SerializeField] private TwinBossManager _twinBossManager;
        [SerializeField] private GameObject _beliaObject;
        [SerializeField] private GameObject _darkTherionObject;
        [SerializeField] private BossUI _beliaUI;
        [SerializeField] private BossUI _darkTherionUI;
        [SerializeField, Min(0)] private float _intermissionTime = 1f;

        [Header("Final Boss")]
        [SerializeField] private GameObject _werbellionPackage;
        [SerializeField] private GameObject _werbellionObject;
        [SerializeField] private BossUI _werbellionUI;

        [Header("State Disply")]
        [SerializeField, TextArea(3, 10)]
        private string _stateDisplay = string.Empty;
        private readonly StringBuilder _sb = new();

        // Internal
        private IBoss _belia;
        private IBoss _darkTherion;
        private IBoss _werbellion;

        private enum Phase
        {
            TwinBossReady,
            TwinBoss,
            Intermission,
            FinalBossReady,
            FinalBoss,
            StageCompleted
        }
        private Phase _phase;


        // Front
        protected override void Awake()
        {
            AutoBindSceneMonsters = false;
            base.Awake();

            if (!_twinBossManager)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_twinBossManager)}이(가) 유효하지 않습니다.'"));

            if (!_beliaObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_beliaObject)}이(가) 유효하지 않습니다.'"));

            if (!_beliaObject.TryGetComponent(out _belia))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_beliaObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_darkTherionObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionObject)}이(가) 유효하지 않습니다.'"));

            if (!_darkTherionObject.TryGetComponent(out _darkTherion))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_beliaUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_beliaUI)}이(가) 유효하지 않습니다.'"));

            if (!_darkTherionUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_darkTherionUI)}이(가) 유효하지 않습니다.'"));

            if (!_werbellionPackage)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_werbellionPackage)}이(가) 유효하지 않습니다.'"));

            if (!_werbellionObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_werbellionObject)}이(가) 유효하지 않습니다.'"));

            if (!_werbellionObject.TryGetComponent(out _werbellion))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_werbellionObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_werbellionUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_werbellionUI)}이(가) 유효하지 않습니다.'"));
        }

        protected override void Start()
        {
            base.Start();
            StartInitial();
        }

        private void RegisterBoss(IBoss boss, BossUI ui, bool createUI = true)
        {
            if (!MonsterManager.Register(boss))
                return;

            if (boss is IPlayerInitializable playerInitializable)
            {
                if (_playerObject
                    && _playerObject.TryGetComponent<IPlayer>(out var player))
                    playerInitializable.InitializePlayer(player);
                else
                    Debug.LogWarning(Ctx(
                        $"{nameof(boss)}에 {nameof(_playerObject)}을(를) 등록하지 못했습니다. " +
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
                StartWerbellion();
            else if (_phase > Phase.FinalBossReady)
                Debug.LogWarning(
                    $"{nameof(_phase)}이(가) '{_phase}'이기 때문에 더 이상의 {nameof(Commence)}이(가) 불가능합니다.",
                    this);
        }

        private void StartInitial()
        {
            _twinBossManager.gameObject.SetActive(true);
            _werbellionPackage.SetActive(false);

            _beliaUI.SetToDisabled();
            _darkTherionUI.SetToDisabled();

            _phase = Phase.TwinBossReady;
        }

        private void StartTwinBoss()
        {
            RegisterBoss(_belia, _beliaUI);
            RegisterBoss(_darkTherion, _darkTherionUI);

            _beliaUI.Enable();
            _darkTherionUI.Enable();

            _twinBossManager.Cleared += () =>
            {
                _phase = Phase.Intermission;

                _beliaUI.Disable();
                _darkTherionUI.Disable();

                new Timer(_intermissionTime, _ =>
                {
                    _werbellionPackage.SetActive(true);
                    _phase = Phase.FinalBossReady;
                });
            };

            _twinBossManager.Commence();
            _phase = Phase.TwinBoss;
        }

        private void StartWerbellion()
        {
            _twinBossManager.gameObject.SetActive(false);
            RegisterBoss(_werbellion, _werbellionUI);

            IDisposable handle = null;
            int frameCount = 1;

            handle = Loco.Subscribe(() =>
            {
                frameCount--;
                if (frameCount > 0) return;

                _werbellion.ConditionChanged += c =>
                {
                    if (c.Is(MonsterCondition.Die))
                        StageCleared();
                };

                _werbellionUI.Enable();
                _werbellion.Commence();
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
        [CustomEditor(typeof(SingleBossStageManager)), CanEditMultipleObjects]
        protected class BossStageManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawPropertiesExcluding(serializedObject, "AutoBindSceneMonsters");

                if (GUILayout.Button("Commence"))
                    ((SingleBossStageManager)target).Commence();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
