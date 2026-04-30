using System;
using System.Linq;
using Actors;
using Actors.Monsters.Bosses;
using BlackboxSystem;
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
    public class StageManager : MonoBehaviour
    {
        [field: Tooltip("������ ���۵� �� �ʿ��� ���� ��ҵ��� ������ ã�� �ڵ����� ����մϴ�")]
        [field: SerializeField] protected bool AutoBindDependencies { get; set; } = true;
        [field: Space]
        [field: Tooltip("������ ���۵� �� ���� �����ϴ� Active ������ �÷��̾ �ڵ����� ����մϴ�")]
        [field: SerializeField] protected bool AutoBindScenePlayer { get; set; } = true;
        [field: Tooltip("������ ���۵� �� ���� �����ϴ� Active ������ ���͵��� �ڵ����� ����մϴ�")]
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
                            $"������ {nameof(canvas)}��(��) ã�� �� �����߽��ϴ�."), this);
                    }
                }
                if (!UIManager.HasWorldUI)
                {
                    var worldUI = GameObject.FindWithTag("WorldUI")?.GetComponent<RectTransform>();
                    if (worldUI)
                    {
                        BlackboxHandle.Of(this).Exert(UIManager, $"Set WorldUI: {worldUI}");
                        UIManager.SetWorldUI(worldUI);
                    }
                    else
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"������ {nameof(worldUI)}��(��) ã�� �� �����߽��ϴ�."), this);
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
                            BlackboxHandle.Of(this).Write("���� �� �̻��� �̺�Ʈ �ý����� �����մϴ�.");
                            isHeaderLogged = true;
                        }

                        BlackboxHandle.Of(this).Write($"�̺�Ʈ �ý��� ����: {es.name}");
                        Destroy(es.gameObject);
                    }
                }

                if (!_sfxPlayManager)
                {
                    _sfxPlayManager = FindAnyObjectByType<SfxPlayManager>(FindObjectsInactive.Include);
                    if (!_sfxPlayManager)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"������ {nameof(_sfxPlayManager)}��(��) ã�� �� �����߽��ϴ�."), this);
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
                            $"������ {nameof(_platformManager)}��(��) ã�� �� �����߽��ϴ�."), this);
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
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"������ {nameof(DarkscreenUI)}��(��) ã�� �� �����߽��ϴ�."), this);
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
                    throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                        $"[{nameof(StageManager)}] {nameof(Player)}��(��) ã�� �� �����߽��ϴ�."));
            }

            if (_playerObject)
            {
                if (!_playerObject.TryGetComponent<IPlayer>(out var player))
                {
                    throw new InvalidOperationException(
                        BlackboxHandle.Of(this).WriteError(
                            $"[{nameof(StageManager)}] {nameof(_playerObject)}��(��) {nameof(IPlayer)} ������Ʈ�� ������ ���� �ʽ��ϴ�."));
                }

                Player = player;
                Register(Player, PlayerUI != null);

                Player.ConditionChanged += cond =>
                {
                    if (cond == PlayerCondition.Die)
                    {
                        new Timer(
                            0.3f,
                            _ => BgmPlayManager.Stop());

                        new Timer(
                            1.3f,
                            _ => DarkscreenUI.CloseScreen(() => PlayerDied?.Invoke()));
                    }
                };
            }
            else
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"[{nameof(StageManager)}] {nameof(_playerObject)}��(��) ��ȿ���� �ʽ��ϴ�."));
            }

            if (_rubielObject)
            {
                if (!_rubielObject.TryGetComponent<Rubiel>(out var rubiel))
                {
                    throw new InvalidOperationException(
                        BlackboxHandle.Of(this).WriteError(
                            $"[{nameof(StageManager)}] {nameof(_rubielObject)}��(��) {nameof(Rubiel)} ������Ʈ�� ������ ���� �ʽ��ϴ�."));
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
                BlackboxHandle.Of(this).Exert(LevelManager.Instance.SpawnManager, "Spawner�� Register �븮�� ���");
                LevelManager.Instance.SpawnManager.OnMonsterCreate(monster => Register(monster));
            }
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "[StageManager] LevelManager.Instance.SpawnManager�� ��ȿ���� �ʽ��ϴ�. " +
                    "���� �����Ǵ� ���ʹ� �Ŵ����� ��ϵ��� ������, UI ���� �������� ���� �� �ֽ��ϴ�."),
                    this);

            if (_dialogueManager)
            {
                BlackboxHandle.Of(this).Exert(_dialogueManager, "DialogueManager �ʱ�ȭ");
                _dialogueManager.Initialize(
                    DialogueUI,
                    BubbleDialogueUI,
                    Canvas.GetComponent<RectTransform>(),
                    character => () => character switch
                    {
                        Character.Player => Player?.transform.position ?? default,
                        Character.Rubiel => Rubiel?.AnchorPos ?? default,
                        Character.Belia => FindAnyObjectByType<Belia>()?.transform.position ?? default,
                        Character.DarkTherion => FindAnyObjectByType<DarkTherion>()?.transform.position ?? default,
                        Character.Werbellion => FindAnyObjectByType<Werbellion>()?.transform.position ?? default,

                        _ => throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                                $"[StageManager] ĳ���� {character}�� Ÿ���� ��ȿ���� �ʽ��ϴ�."))
                    });
            }
            #endregion

            // Add Input Subjects
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

            if (_portal)
                _portal.MoveToNextLevel += callback =>
                {
                    BlackboxHandle.Of(this).Exert(InputHub, "Block (_portal.MoveToNextLevel)");
                    InputHub.Block(this);

                    DarkscreenUI.CloseScreen(callback);
                };

            if (DialogueUI)
            {
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

                ((IInputLayerController)RelicAcquisitionUI).Initialize(InputHub);
            }
            if (RelicInfoPanelUI)
                ((IInputLayerController)RelicInfoPanelUI).Initialize(InputHub);
            if (SettingsUI)
            {
                ((IInputLayerController)SettingsUI).Initialize(InputHub);

                SettingsUI.RestartUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(SettingsUI, "SettingsUI -> RestartUI ��û ó��");

                    BgmPlayManager.Stop();
                    DarkscreenUI.CloseScreen(() => PlayerDied?.Invoke());
                };

                SettingsUI.OpenRelicsUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(SettingsUI, "SettingsUI -> OpenRelicsUI ��û ó��");
                    if (!RelicInfoPanelUI)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            "[StageManager] RelicInfoPanelUI�� �Ҵ���� �ʾ� �ش� â�� �� �� �����ϴ�."), this);
                        return;
                    }

                    BlackboxHandle.Of(this).Exert(RelicInfoPanelUI, "RelicsUI ����");
                    RelicInfoPanelUI.Open();
                };

                SettingsUI.OpenGuideUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(SettingsUI, "SettingsUI -> OpenGuideUI ��û ó��");
                    if (!GuideAndWorldRecordsUI)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            "[StageManager] GuideAndWorldRecordsUI�� �Ҵ���� �ʾ� �ش� â�� �� �� �����ϴ�."), this);
                        return;
                    }

                    BlackboxHandle.Of(this).Exert(GuideAndWorldRecordsUI, "GuideUI ����");
                    GuideAndWorldRecordsUI.Open();
                };
            }
            if (GuideAndWorldRecordsUI)
                ((IInputLayerController)GuideAndWorldRecordsUI).Initialize(InputHub);


            foreach (var controlObj in _additionalInputControllers.Concat(_additionalInputControllables))
            {
                if (controlObj is IInputLayerSubject subject)
                {
                    BlackboxHandle.Of(this).Exert(subject, $"���: {controlObj.name}");
                    InputHub.Add(subject);
                }
                if (controlObj is IInputLayerController controller)
                {
                    BlackboxHandle.Of(this).Exert(controller, $"�ʱ�ȭ: {controlObj.name}");
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
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(Rubiel)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!_portal)
            {
                _portal = FindAnyObjectByType<Portal>();
                if (!_portal)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(Portal)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!Canvas)
            {
                Canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
                if (!Canvas)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(Canvas)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(PlayerUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!RelicAcquisitionUI)
            {
                RelicAcquisitionUI = FindAnyObjectByType<RelicAcquisitionUI>(FindObjectsInactive.Include);
                if (!RelicAcquisitionUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(RelicAcquisitionUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!RelicInfoPanelUI)
            {
                RelicInfoPanelUI = FindAnyObjectByType<RelicInfoPanelUI>(FindObjectsInactive.Include);
                if (!RelicInfoPanelUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(RelicInfoPanelUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!PlayerUI)
            {
                PlayerUI = FindAnyObjectByType<PlayerUI>(FindObjectsInactive.Include);
                if (!PlayerUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(PlayerUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!SettingsUI)
            {
                SettingsUI = FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
                if (!SettingsUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(SettingsUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!DialogueUI)
            {
                DialogueUI = FindAnyObjectByType<DialogueUI>(FindObjectsInactive.Include);
                if (!DialogueUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(DialogueUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!BubbleDialogueUI)
            {
                BubbleDialogueUI = FindAnyObjectByType<BubbleDialogueUI>(FindObjectsInactive.Include);
                if (!BubbleDialogueUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(BubbleDialogueUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }

            if (!GuideAndWorldRecordsUI)
            {
                GuideAndWorldRecordsUI = FindAnyObjectByType<GuideAndWorldRecordsUI>(FindObjectsInactive.Include);
                if (!GuideAndWorldRecordsUI)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"������ {nameof(GuideAndWorldRecordsUI)}��(��) ã�� �� �����߽��ϴ�."), this);
                }
            }
        }

        public void Register(IPlayer player, bool connectUI = true)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(PlayerManager, $"PlayerManager�� Player ���, _isDestroyed: {_isDestroyed}");
            if (_isDestroyed) return;

            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "UIManager�� PlayerUI ���");

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

            using var _ = BlackboxHandle.Of(this).ExertScope(monster, "MonsterManager�� Monster ���");

            if (!MonsterManager.Register(monster))
                return;

            if (createUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "UIManager�� MonsterUI ���");

                var vm = new MonsterVM(monster);
                var ui = UILibrary.HealthBar;
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui, true);
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
