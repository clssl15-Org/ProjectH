using System.Linq;
using Actors;
using Actors.Monsters.Bosses;
using BlackboxSystem;
using Dialogue;
using Infrastructure;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using World;

namespace Game.Stage
{
    [RequireComponent(typeof(Injector), typeof(InputHub), typeof(UIManager))]
    [RequireComponent(typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour
    {
        [field: Tooltip("게임이 시작될 때 필요한 구성 요소들을 씬에서 찾아 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindDependencies { get; set; } = true;
        [field: Space]
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindScenePlayer { get; set; } = false;
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindSceneMonsters { get; set; } = true;

        [Header("Bindings")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private UILibrary _uILibrary;
        [SerializeField] private EventSystem _eventSystem;

        [Header("Inputs")]
        [SerializeField] private MonoBehaviour[] _additionalInputControllers;
        [SerializeField] private MonoBehaviour[] _additionalInputControllables;

        [Header("Player")]
        [SerializeField] private GameObject _playerObject;
        [SerializeField] private GameObject _rubielObject;

        [field: Header("UIs")]
        [field: SerializeField] internal Canvas Canvas { get; private set; }
        [field: SerializeField] internal PlayerUI PlayerUI { get; private set; }
        [field: SerializeField] internal RelicAcquisitionUI RelicAcquisitionUI { get; private set; }
        [field: SerializeField] internal RelicInfoPanelUI RelicInfoPanelUI { get; private set; }
        [field: SerializeField] internal SettingsUI SettingsUI { get; private set; }
        [field: SerializeField] internal DialogueUI DialogueUI { get; private set; }
        [field: SerializeField] internal BubbleDialogueUI BubbleDialogueUI { get; private set; }
        [field: SerializeField] internal GuideAndWorldRecordsUI GuideAndWorldRecordsUI { get; private set; }
        [field: SerializeField] internal DarkscreenUI DarkScreenUI { get; private set; }

        internal IPlayer Player { get; private set; }
        internal Rubiel Rubiel { get; private set; }
        internal UILibrary UILibrary => _uILibrary;

        internal DialogueManager DialogueManager => _dialogueManager;
        internal InputHub InputHub { get; private set; }
        internal UIManager UIManager { get; private set; }
        internal PlayerManager PlayerManager { get; private set; }
        internal MonsterManager MonsterManager { get; private set; }

        private bool _isDestroyed = false;


        // Front
        protected virtual void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

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
                        BlackboxHandle.Of(this).Exert(UIManager, $"Set Canvas: {canvas}");
                        UIManager.SetCanvas(canvas.GetComponent<RectTransform>());
                    }
                    else
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"씬에서 {nameof(canvas)}을(를) 찾는 데 실패했습니다."), this);
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
                            BlackboxHandle.Of(this).Write("씬에 둘 이상의 이벤트 시스템이 존재합니다.");
                            isHeaderLogged = true;
                        }

                        BlackboxHandle.Of(this).Write($"이벤트 시스템 삭제: {es.name}");
                        Destroy(es.gameObject);
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
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"씬에서 {nameof(_platformManager)}을(를) 찾는 데 실패했습니다."), this);
                    }
                }
                if (!injector.HasInjection<DarkscreenUI>())
                {
                    if (!DarkScreenUI)
                        DarkScreenUI = FindAnyObjectByType<DarkscreenUI>(FindObjectsInactive.Include);

                    if (DarkScreenUI)
                    {
                        injector.AddInjection(DarkScreenUI, typeof(DarkscreenUI));
                    }
                    else
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"씬에서 {nameof(DarkScreenUI)}을(를) 찾는 데 실패했습니다."), this);
                    }
                }
            }
        }

        protected virtual void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Start");
            if (AutoBindDependencies) AutoBindDependenciesInScene();

            #region Player / Monsters
            if (AutoBindScenePlayer)
            {
                foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (playerObj
                        && playerObj.activeSelf
                        && playerObj.TryGetComponent<IPlayer>(out var player))
                    {
                        _playerObject = playerObj;
                        Player = player;

                        Register(player, PlayerUI != null);
                        break;
                    }
                }

                var rubiel = FindAnyObjectByType<Rubiel>();
                if (rubiel)
                {
                    _rubielObject = rubiel.gameObject;
                    Rubiel = rubiel;
                }
            }
            else
            {
                if (_playerObject)
                {
                    if (!_playerObject.TryGetComponent<IPlayer>(out var player))
                    {
                        throw new System.InvalidOperationException(
                            BlackboxHandle.Of(this).CrashExport(
                                $"[{nameof(StageManager)}] {nameof(_playerObject)}이(가) {nameof(IPlayer)} 컴포넌트를 가지고 있지 않습니다."));
                    }

                    Player = player;
                    Register(Player, PlayerUI != null);
                }

                if (_rubielObject)
                {
                    if (!_rubielObject.TryGetComponent<Rubiel>(out var rubiel))
                    {
                        throw new System.InvalidOperationException(
                            BlackboxHandle.Of(this).CrashExport(
                                $"[{nameof(StageManager)}] {nameof(_rubielObject)}이(가) {nameof(Rubiel)} 컴포넌트를 가지고 있지 않습니다."));
                    }

                    Rubiel = rubiel;
                }
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
                BlackboxHandle.Of(this).Exert(LevelManager.Instance.SpawnManager, "Spawner에 Register 대리자 등록");
                LevelManager.Instance.SpawnManager.OnMonsterCreate(monster => Register(monster));
            }
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "[StageManager] LevelManager.Instance.SpawnManager가 유효하지 않습니다. " +
                    "새로 스폰되는 몬스터는 매니저에 등록되지 않으며, UI 등이 생성되지 않을 수 있습니다."),
                    this);

            if (_dialogueManager)
            {
                BlackboxHandle.Of(this).Exert(_dialogueManager, "DialogueManager 초기화");
                _dialogueManager.Initialize(
                    DialogueUI,
                    BubbleDialogueUI,
                    Canvas.GetComponent<RectTransform>(),
                    character => character switch
                    {
                        Character.Player => Player?.transform,
                        Character.Rubiel => Rubiel?.transform,
                        Character.Belia => FindAnyObjectByType<Belia>()?.transform,
                        Character.DarkTherion => FindAnyObjectByType<DarkTherion>()?.transform,
                        Character.Werbellion => FindAnyObjectByType<Werbellion>()?.transform,

                        _ => throw new System.InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                                $"[StageManager] 캐릭터 {character}의 타입이 유효하지 않습니다."))
                    });
            }
            #endregion

            #region Input Hub
            // Add Subjects
            if (Player != null)
            {
                BlackboxHandle.Of(this).Exert(InputHub, "Add Player");
                InputHub.Add(Player);
            }
            if (PlayerUI)
            {
                BlackboxHandle.Of(this).Exert(InputHub, "Add PlayerUI");
                InputHub.Add(PlayerUI);
            }
            if (SettingsUI)
            {
                BlackboxHandle.Of(this).Exert(InputHub, "Add SettingsUI (Opener)");
                InputHub.Add(SettingsUI.GetOpenerSubject());
            }
            if (RelicInfoPanelUI)
            {
                BlackboxHandle.Of(this).Exert(InputHub, "Add RelicInfoPanelUI");
                InputHub.Add(RelicInfoPanelUI.GetOpenerSubject());
            }


            // Add Controllers
            if (RelicAcquisitionUI)
                ((IInputLayerController)RelicAcquisitionUI).Initialize(InputHub);
            if (RelicInfoPanelUI)
                ((IInputLayerController)RelicInfoPanelUI).Initialize(InputHub);
            if (SettingsUI)
            {
                ((IInputLayerController)SettingsUI).Initialize(InputHub);

                SettingsUI.OpenRelicsUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(SettingsUI, "SettingsUI -> OpenRelicsUI 요청 처리");
                    if (!RelicInfoPanelUI)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            "[StageManager] RelicInfoPanelUI가 할당되지 않아 해당 창을 열 수 없습니다."), this);
                        return;
                    }

                    BlackboxHandle.Of(this).Exert(RelicInfoPanelUI, "RelicsUI 열기");
                    RelicInfoPanelUI.Open();
                };

                SettingsUI.OpenGuideUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(SettingsUI, "SettingsUI -> OpenGuideUI 요청 처리");
                    if (!GuideAndWorldRecordsUI)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            "[StageManager] GuideAndWorldRecordsUI가 할당되지 않아 해당 창을 열 수 없습니다."), this);
                        return;
                    }

                    BlackboxHandle.Of(this).Exert(GuideAndWorldRecordsUI, "GuideUI 열기");
                    GuideAndWorldRecordsUI.Open();
                };
            }
            if (GuideAndWorldRecordsUI)
                ((IInputLayerController)GuideAndWorldRecordsUI).Initialize(InputHub);

            foreach (var controlObj in _additionalInputControllers.Concat(_additionalInputControllables))
            {
                if (controlObj is IInputLayerSubject subject)
                {
                    BlackboxHandle.Of(this).Exert(subject, $"등록: {controlObj.name}");
                    InputHub.Add(subject);
                }
                if (controlObj is IInputLayerController controller)
                {
                    BlackboxHandle.Of(this).Exert(controller, $"초기화: {controlObj.name}");
                    controller.Initialize(InputHub);
                }
            }
            #endregion
        }
        private void AutoBindDependenciesInScene()
        {
            if (!Canvas)
            {
                Canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
                if (!Canvas)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(Canvas)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(PlayerUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!RelicAcquisitionUI)
            {
                RelicAcquisitionUI = FindAnyObjectByType<RelicAcquisitionUI>(FindObjectsInactive.Include);
                if (!RelicAcquisitionUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(RelicAcquisitionUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!RelicInfoPanelUI)
            {
                RelicInfoPanelUI = FindAnyObjectByType<RelicInfoPanelUI>(FindObjectsInactive.Include);
                if (!RelicInfoPanelUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(RelicInfoPanelUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(PlayerUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!SettingsUI)
            {
                SettingsUI = FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
                if (!SettingsUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(SettingsUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!DialogueUI)
            {
                DialogueUI = FindAnyObjectByType<DialogueUI>(FindObjectsInactive.Include);
                if (!DialogueUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(DialogueUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!BubbleDialogueUI)
            {
                BubbleDialogueUI = FindAnyObjectByType<BubbleDialogueUI>(FindObjectsInactive.Include);
                if (!BubbleDialogueUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(BubbleDialogueUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }

            if (!GuideAndWorldRecordsUI)
            {
                GuideAndWorldRecordsUI = FindAnyObjectByType<GuideAndWorldRecordsUI>(FindObjectsInactive.Include);
                if (!GuideAndWorldRecordsUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"씬에서 {nameof(GuideAndWorldRecordsUI)}을(를) 찾는 데 실패했습니다."), this);
                }
            }
        }

        public void Register(IPlayer player, bool connectUI = true)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(PlayerManager, $"PlayerManager에 Player 등록, _isDestroyed: {_isDestroyed}");
            if (_isDestroyed) return;

            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "UIManager에 PlayerUI 등록");

                var vm = new PlayerVM(player);
                var ui = PlayerUI;
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui);
            }
        }

        public void Register(IMonster monster, bool createUI = true)
        {
            if (_isDestroyed)
                return;

            using var _ = BlackboxHandle.Of(this).ExertScope(monster, "MonsterManager에 Monster 등록");

            if (!MonsterManager.Register(monster))
                return;

            if (createUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "UIManager에 MonsterUI 등록");

                var vm = new MonsterVM(monster);
                var ui = UILibrary.HealthBar;
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui);
            }
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;

            if (LevelManager.Instance?.SpawnManager)
            {
                BlackboxHandle.Of(this).Exert(LevelManager.Instance.SpawnManager, "Clear");
                LevelManager.Instance.SpawnManager.Clear();
            }

            if (UIManager)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "Destroy");
                UIManager.Destroy();
            }

            if (PlayerManager)
            {
                BlackboxHandle.Of(this).Exert(PlayerManager, "Destroy");
                PlayerManager.Destroy();
            }

            if (MonsterManager)
            {
                BlackboxHandle.Of(this).Exert(MonsterManager, "Destroy");
                MonsterManager.Destroy();
            }
        }
    }
}
