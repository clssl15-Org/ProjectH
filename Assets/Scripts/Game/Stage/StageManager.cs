using System;
using System.Linq;
using Actors;
using Actors.Monsters.Bosses;
using BlackThunder.BlackboxSystem;
using Dialogue;
using Infrastructure;
using Sound;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using World;

namespace Game.Stage
{
    [RequireComponent(typeof(Injector), typeof(InputHub), typeof(UIManager))]
    [RequireComponent(typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour,
        IInjectable<SfxPlayManager>
    {
        [field: Tooltip("게임이 시작될 때 필요한 구성 요소들을 씬에서 찾아 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindDependencies { get; set; } = true;
        [field: Space]
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindScenePlayer { get; set; } = true;
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindSceneMonsters { get; set; } = true;

        [Header("Bindings")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private UILibrary _uILibrary;
        [SerializeField] private SfxPlayManager _sfxPlayManager;
        [SerializeField] private EventSystem _eventSystem;

        [Header("Inputs")]
        [SerializeField] private MonoBehaviour[] _additionalInputControllers;
        [SerializeField] private MonoBehaviour[] _additionalInputControllables;

        [Header("World")]
        [SerializeField] private GameObject _playerObject;
        [SerializeField] private GameObject _rubielObject;
        [SerializeField] private Box _box;
        [SerializeField] private Portal _portal;

        [field: Header("UIs")]
        [field: SerializeField] internal Canvas Canvas { get; private set; }
        [field: SerializeField] internal PlayerUI PlayerUI { get; private set; }
        [field: SerializeField] internal RelicAcquisitionUI RelicAcquisitionUI { get; private set; }
        [field: SerializeField] internal RelicInfoPanelUI RelicInfoPanelUI { get; private set; }
        [field: SerializeField] internal SettingsUI SettingsUI { get; private set; }
        [field: SerializeField] internal DialogueUI DialogueUI { get; private set; }
        [field: SerializeField] internal BubbleDialogueUI BubbleDialogueUI { get; private set; }
        [field: SerializeField] internal GuideAndWorldRecordsUI GuideAndWorldRecordsUI { get; private set; }
        [field: SerializeField] internal DarkscreenUI DarkscreenUI { get; private set; }

        [field: Header("Settings")]
        [field: SerializeField] private bool _fadeInOnStart = true;
        [field: SerializeField] private BgmName _bgm = BgmName.None;

        public Action PlayerDied;

        internal IPlayer Player { get; private set; }
        internal Rubiel Rubiel { get; private set; }
        internal UILibrary UILibrary => _uILibrary;

        public Box Box => _box;
        public Portal Portal => _portal;

        internal DialogueManager DialogueManager => _dialogueManager;
        internal InputHub InputHub { get; private set; }
        internal UIManager UIManager { get; private set; }
        internal PlayerManager PlayerManager { get; private set; }
        internal MonsterManager MonsterManager { get; private set; }

        private CursorVisibilityController _cursorVisibilityController;
        private bool _isDestroyed = false;
        private BlackboxHandle _blackbox;


        // Front
        void IInjectable<SfxPlayManager>.Inject(SfxPlayManager sfxPlayManager)
        {
            if (!sfxPlayManager) return;

            _sfxPlayManager = sfxPlayManager;
        }

        protected virtual void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("스테이지 매니저 초기화를 시작합니다.", out _blackbox);

            InputHub = GetComponent<InputHub>();
            UIManager = GetComponent<UIManager>();
            PlayerManager = GetComponent<PlayerManager>();
            MonsterManager = GetComponent<MonsterManager>();

            // Auto Bindings
            if (AutoBindDependencies)
            {
                if (!UIManager.HasCanvas)
                {
                    var canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
                    if (canvas)
                    {
                        UIManager.SetCanvas(canvas.GetComponent<RectTransform>());
                    }
                    else
                    {
                        Debug.LogWarning($"씬에서 {nameof(canvas)}을(를) 찾는 데 실패했습니다.", this);
                    }
                }
                if (!UIManager.HasWorldUI)
                {
                    var worldUI = GameObject.FindWithTag("WorldUI")?.GetComponent<RectTransform>();
                    if (worldUI)
                    {
                        UIManager.SetWorldUI(worldUI);
                    }
                    else
                    {
                        Debug.LogWarning($"씬에서 {nameof(worldUI)}을(를) 찾는 데 실패했습니다.", this);
                    }
                }

                var allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
                if (allEventSystems.Length > 1)
                {
                    var isHeaderLogged = false;

                    foreach (var es in allEventSystems)
                    {
                        if (es == _eventSystem)
                            continue;

                        if (!isHeaderLogged)
                        {
                            isHeaderLogged = true;
                        }

                        Destroy(es.gameObject);
                    }
                }

                if (!_sfxPlayManager)
                {
                    _sfxPlayManager = FindAnyObjectByType<SfxPlayManager>(FindObjectsInactive.Include);
                    if (!_sfxPlayManager)
                    {
                        Debug.LogWarning($"씬에서 {nameof(_sfxPlayManager)}을(를) 찾는 데 실패했습니다.", this);
                    }
                }

                var injector = GetComponent<Injector>();
                if (!injector.HasInjection<PlatformManager>())
                {
                    if (!_platformManager)
                        _platformManager = FindAnyObjectByType<PlatformManager>(FindObjectsInactive.Include);

                    if (_platformManager)
                    {
                        injector.AddInjection(_platformManager, typeof(PlatformManager));
                    }
                    else
                    {
                        Debug.LogWarning($"씬에서 {nameof(_platformManager)}을(를) 찾는 데 실패했습니다.", this);
                    }
                }
                if (!injector.HasInjection<DarkscreenUI>())
                {
                    if (!DarkscreenUI)
                        DarkscreenUI = FindAnyObjectByType<DarkscreenUI>(FindObjectsInactive.Include);

                    if (DarkscreenUI)
                    {
                        injector.AddInjection(DarkscreenUI, typeof(DarkscreenUI));
                    }
                    else
                    {
                        Debug.LogWarning($"씬에서 {nameof(DarkscreenUI)}을(를) 찾는 데 실패했습니다.", this);
                    }
                }
            }
        }

        protected virtual void Start()
        {
            using var _ = _blackbox.Scope("스테이지 구성을 시작합니다.");

            if (AutoBindDependencies) AutoBindDependenciesInScene();

            _cursorVisibilityController = new CursorVisibilityController();
            _cursorVisibilityController.ResetToGameplay();

            #region Player / Monsters
            if (AutoBindScenePlayer)
            {
                var found = false;
                foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (playerObj
                        && playerObj.activeSelf
                        && playerObj.TryGetComponent<IPlayer>(out var player))
                    {
                        _playerObject = playerObj;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    throw new InvalidOperationException($"[{nameof(StageManager)}] {nameof(Player)}을(를) 찾는 데 실패했습니다.");
            }

            if (_playerObject)
            {
                if (!_playerObject.TryGetComponent<IPlayer>(out var player))
                {
                    throw new InvalidOperationException(
                        $"[{nameof(StageManager)}] {nameof(_playerObject)}이(가) {nameof(IPlayer)} 컴포넌트를 가지고 있지 않습니다.");
                }

                Player = player;
                Register(Player, PlayerUI != null);

                Player.ConditionChanged += cond =>
                {
                    if (cond == PlayerCondition.Die)
                    {
                        new Timer(
                            0.3f,
                            timer => BgmPlayManager.Stop());

                        new Timer(
                            1.3f,
                            timer => DarkscreenUI.CloseScreen(() => PlayerDied?.Invoke()));
                    }
                };
            }
            else
            {
                throw new InvalidOperationException($"[{nameof(StageManager)}] {nameof(_playerObject)}이(가) 유효하지 않습니다.");
            }

            if (_rubielObject)
            {
                if (!_rubielObject.TryGetComponent<Rubiel>(out var rubiel))
                {
                    throw new InvalidOperationException(
                        $"[{nameof(StageManager)}] {nameof(_rubielObject)}이(가) {nameof(Rubiel)} 컴포넌트를 가지고 있지 않습니다.");
                }

                Rubiel = rubiel;
            }

            if (AutoBindSceneMonsters)
                foreach (var monsterObject in GameObject.FindGameObjectsWithTag("Monster"))
                {
                    if (!monsterObject.TryGetComponent<IMonster>(out var monster))
                        continue;

                    Register(monster);
                }
            #endregion

            #region Managers
            if (LevelManager.Instance?.SpawnManager)
            {
                _blackbox.Write("스폰 매니저 몬스터 생성 콜백을 연결합니다.").With(LevelManager.Instance.SpawnManager);
                LevelManager.Instance.SpawnManager.OnMonsterCreate(monster => Register(monster));
            }
            else
                Debug.LogWarning("[StageManager] LevelManager.Instance.SpawnManager가 유효하지 않습니다. " +
                    "새로 스폰되는 몬스터는 매니저에 등록되지 않으며, UI 등이 생성되지 않을 수 있습니다.",
                    this);

            if (_dialogueManager)
            {
                using (_blackbox.Exert(_dialogueManager, "대화 매니저 초기화를 요청합니다."))
                _dialogueManager.Initialize(
                    DialogueUI,
                    BubbleDialogueUI,
                    Canvas.GetComponent<RectTransform>(),
                    character => () => character switch
                    {
                        Character.Player => Player?.transform.position ?? default,
                        Character.Rubiel => Rubiel?.AnchorPos ?? default,
                        Character.Velia => FindAnyObjectByType<Velia>()?.transform.position ?? default,
                        Character.DarkTherion => FindAnyObjectByType<DarkTherion>()?.transform.position ?? default,
                        Character.Verbelion => FindAnyObjectByType<Verbelion>()?.transform.position ?? default,

                        _ => throw new InvalidOperationException($"[StageManager] 캐릭터 {character}의 타입이 유효하지 않습니다.")
                    });
            }
            #endregion

            // Add Input Subjects
            if (Player != null)
            {
                InputHub.Add(Player);
            }
            if (PlayerUI)
            {
                InputHub.Add(PlayerUI);
            }
            if (SettingsUI)
            {
                InputHub.Add(SettingsUI.GetOpenerSubject());
            }
            if (RelicInfoPanelUI)
            {
                InputHub.Add(RelicInfoPanelUI.GetOpenerSubject());
            }

            if (_portal)
                _portal.MoveToNextLevel += callback =>
                {
                    InputHub.Block(this);

                    DarkscreenUI.CloseScreen(callback);
                };

            if (DialogueUI)
            {
                ((ICursorVisibilityControllerUser)DialogueUI).Initialize(_cursorVisibilityController);

                if (_sfxPlayManager)
                    DialogueUI.TextTyped += () => _sfxPlayManager.Play(SfxName.Text);
            }
            if (BubbleDialogueUI)
            {
                if (_sfxPlayManager)
                    BubbleDialogueUI.TextTyped += () => _sfxPlayManager.Play(SfxName.Text);
            }
            if (RelicAcquisitionUI)
            {
                if (_sfxPlayManager)
                {
                    RelicAcquisitionUI.CoinThrown += () => _sfxPlayManager.Play(SfxName.CoinThrow);
                    RelicAcquisitionUI.CoinDropped += () => _sfxPlayManager.Play(SfxName.CoinDrop);
                }

                RelicAcquisitionUI.Disabling += () => RelicManager.Instance?.NotifyAcquisitionUiClosed();

                ((IInputLayerController)RelicAcquisitionUI).Initialize(InputHub);
                ((ICursorVisibilityControllerUser)RelicAcquisitionUI).Initialize(_cursorVisibilityController);
            }
            if (RelicInfoPanelUI)
            {
                ((IInputLayerController)RelicInfoPanelUI).Initialize(InputHub);
                ((ICursorVisibilityControllerUser)RelicInfoPanelUI).Initialize(_cursorVisibilityController);
            }
            if (SettingsUI)
            {
                ((IInputLayerController)SettingsUI).Initialize(InputHub);
                ((ICursorVisibilityControllerUser)SettingsUI).Initialize(_cursorVisibilityController);

                SettingsUI.RestartUI += () =>
                {

                    BgmPlayManager.Stop();
                    DarkscreenUI.CloseScreen(() => PlayerDied?.Invoke());
                };

                SettingsUI.OpenRelicsUI += () =>
                {
                    if (!RelicInfoPanelUI)
                    {
                        Debug.LogWarning("[StageManager] RelicInfoPanelUI가 할당되지 않아 해당 창을 열 수 없습니다.", this);
                        return;
                    }

                    RelicInfoPanelUI.Open();
                };

                SettingsUI.OpenGuideUI += () =>
                {
                    if (!GuideAndWorldRecordsUI)
                    {
                        Debug.LogWarning("[StageManager] GuideAndWorldRecordsUI가 할당되지 않아 해당 창을 열 수 없습니다.", this);
                        return;
                    }

                    GuideAndWorldRecordsUI.Open();
                };
            }
            if (GuideAndWorldRecordsUI)
            {
                ((IInputLayerController)GuideAndWorldRecordsUI).Initialize(InputHub);
                ((ICursorVisibilityControllerUser)GuideAndWorldRecordsUI).Initialize(_cursorVisibilityController);
            }


            foreach (var controlObj in _additionalInputControllers.Concat(_additionalInputControllables))
            {
                if (controlObj is IInputLayerSubject subject)
                {
                    InputHub.Add(subject);
                }
                if (controlObj is IInputLayerController controller)
                {
                    controller.Initialize(InputHub);
                }
            }


            // Setups
            if (_fadeInOnStart)
                DarkscreenUI.OpenScreen();

            if (_bgm != BgmName.None)
                BgmPlayManager.Play(_bgm);
        }
        private void AutoBindDependenciesInScene()
        {
            if (!_rubielObject)
            {
                _rubielObject = FindAnyObjectByType<Rubiel>(FindObjectsInactive.Include)?.gameObject;
                if (!_rubielObject)
                {
                    Debug.LogWarning($"씬에서 {nameof(Rubiel)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!_portal)
            {
                _portal = FindAnyObjectByType<Portal>();
                if (!_portal)
                {
                    Debug.LogWarning($"씬에서 {nameof(Portal)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!Canvas)
            {
                Canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
                if (!Canvas)
                {
                    Debug.LogWarning($"씬에서 {nameof(Canvas)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(PlayerUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!RelicAcquisitionUI)
            {
                RelicAcquisitionUI = FindAnyObjectByType<RelicAcquisitionUI>(FindObjectsInactive.Include);
                if (!RelicAcquisitionUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(RelicAcquisitionUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!RelicInfoPanelUI)
            {
                RelicInfoPanelUI = FindAnyObjectByType<RelicInfoPanelUI>(FindObjectsInactive.Include);
                if (!RelicInfoPanelUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(RelicInfoPanelUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(PlayerUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!SettingsUI)
            {
                SettingsUI = FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
                if (!SettingsUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(SettingsUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!DialogueUI)
            {
                DialogueUI = FindAnyObjectByType<DialogueUI>(FindObjectsInactive.Include);
                if (!DialogueUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(DialogueUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!BubbleDialogueUI)
            {
                BubbleDialogueUI = FindAnyObjectByType<BubbleDialogueUI>(FindObjectsInactive.Include);
                if (!BubbleDialogueUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(BubbleDialogueUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }

            if (!GuideAndWorldRecordsUI)
            {
                GuideAndWorldRecordsUI = FindAnyObjectByType<GuideAndWorldRecordsUI>(FindObjectsInactive.Include);
                if (!GuideAndWorldRecordsUI)
                {
                    Debug.LogWarning($"씬에서 {nameof(GuideAndWorldRecordsUI)}을(를) 찾는 데 실패했습니다.", this);
                }
            }
        }

        public void Register(IPlayer player, bool connectUI = true)
        {
            using var _ = _blackbox.Scope($"플레이어 등록을 시작합니다. connectUI: {connectUI}").With(player);

            if (_isDestroyed) return;

            bool isRegistered;
            using (_blackbox.Exert(PlayerManager, "플레이어 매니저에 플레이어 등록을 요청합니다."))
            isRegistered = PlayerManager.Register(player);

            if (!isRegistered)
            {
                _blackbox.Write("이미 등록된 플레이어라 추가 처리를 건너뜁니다.").With(player);
                return;
            }

            if (connectUI)
            {

                var vm = new PlayerVM(player);
                var ui = PlayerUI;
                ui.Connect(vm);

                using (_blackbox.Exert(UIManager, "플레이어 UI 등록을 요청합니다."))
                {
                    UIManager.RegisterVM(vm);
                    UIManager.RegisterView(ui);
                }
            }
        }

        public void Register(IMonster monster, bool createUI = true)
        {
            using var _ = _blackbox.Scope($"몬스터 등록을 시작합니다. createUI: {createUI}").With(monster);

            if (_isDestroyed)
                return;


            bool isRegistered;
            using (_blackbox.Exert(MonsterManager, "몬스터 매니저에 몬스터 등록을 요청합니다."))
            isRegistered = MonsterManager.Register(monster);

            if (!isRegistered)
            {
                _blackbox.Write("이미 등록된 몬스터라 추가 처리를 건너뜁니다.").With(monster);
                return;
            }

            if (createUI)
            {

                var vm = new MonsterVM(monster);
                var ui = UILibrary.HealthBar;
                ui.Connect(vm);

                using (_blackbox.Exert(UIManager, "몬스터 UI 등록을 요청합니다."))
                {
                    UIManager.RegisterVM(vm);
                    UIManager.RegisterView(ui, true);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            using var _ = _blackbox.Scope("스테이지 매니저를 정리합니다.");

            _isDestroyed = true;
            _cursorVisibilityController?.RestoreVisible();

            if (LevelManager.Instance?.SpawnManager)
            {
                LevelManager.Instance.SpawnManager.Clear();
            }

            if (UIManager)
            {
                UIManager.Destroy();
            }

            if (PlayerManager)
            {
                PlayerManager.Destroy();
            }

            if (MonsterManager)
            {
                MonsterManager.Destroy();
            }
        }
    }
}
