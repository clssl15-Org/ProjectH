using System.Linq;
using Actors;
using BlackboxSystem;
using Infrastructure;
using UI;
using UnityEngine;

namespace Game.Stage
{
    [RequireComponent(typeof(InputHub), typeof(UIManager))]
    [RequireComponent(typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour
    {        
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindScenePlayer { get; set; } = false;
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindSceneMonsters { get; set; } = true;

        [Header("Bindings")]
        [SerializeField] private UILibrary _uILibrary;

        [Header("Inputs")]
        [SerializeField] private MonoBehaviour[] _additionalInputControllers;
        [SerializeField] private MonoBehaviour[] _additionalInputControllables;

        [Header("Player")]
        [SerializeField] private GameObject _playerObject;

        [Header("UIs")]
        [SerializeField] private PlayerUI _playerUI;
        [SerializeField] private RelicAcquisitionUI _relicAcquisitionUI;
        [SerializeField] private RelicInfoPanelUI _relicInfoPanelUI;
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private DialogueUI _dialogueUI;

        protected IPlayer Player { get; private set; }
        protected UILibrary UILibrary => _uILibrary;

        protected InputHub InputHub { get; private set; }
        protected UIManager UIManager { get; private set; }
        protected PlayerManager PlayerManager { get; private set; }
        protected MonsterManager MonsterManager { get; private set; }

        private bool _isDestroyed = false;


        // Front
        protected virtual void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            InputHub = GetComponent<InputHub>();
            UIManager = GetComponent<UIManager>();
            PlayerManager = GetComponent<PlayerManager>();
            MonsterManager = GetComponent<MonsterManager>();
        }

        protected virtual void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Start");

            #region Player / Monsters
            if (AutoBindScenePlayer)
            {
                foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (playerObj
                        && playerObj.activeSelf
                        && playerObj.TryGetComponent<IPlayer>(out var player))
                    {
                        Player = player;
                        Register(player, _playerUI != null);

                        break;
                    }
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
                    Register(Player, _playerUI != null);
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

            #region Spawn Manager
            if (SpawnManager.Instance)
            {
                BlackboxHandle.Of(this).Exert(SpawnManager.Instance, "Spawner에 Register 대리자 등록");
                SpawnManager.Instance.OnMonsterCreate(monster => Register(monster));
            }
            else
                Debug.LogWarning(BlackboxHandle.Of(this).Write(
                    "[StageManager] SpawnManager.Instance이(가) 유효하지 않습니다. " +
                    "새로 스폰되는 몬스터는 매니저에 등록되지 않으며, UI 등이 생성되지 않을 수 있습니다."),
                    this);
            #endregion

            #region Input Hub
            if (Player != null)
                InputHub.Register(Player);
            if (_playerUI)
                InputHub.Register(_playerUI);

            if (_relicAcquisitionUI)
                ((IInputController)_relicAcquisitionUI).Initialize(InputHub);
            if (_relicInfoPanelUI)
                ((IInputController)_relicInfoPanelUI).Initialize(InputHub);
            if (_settingsUI)
            {
                InputHub.Register(_settingsUI);
                ((IInputController)_settingsUI).Initialize(InputHub);

                _settingsUI.OpenRelicsUI += () =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertScope(_settingsUI, "_settingsUI -> RelicsUI 열기 요청 처리");
                    if (!_relicInfoPanelUI)
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).Write(
                            "[StageManager] _relicInfoPanelUI가 할당되지 않아 RelicsUI를 열 수 없습니다."), this);
                        return;
                    }

                    // UI를 최상위 창으로 열기
                    _relicInfoPanelUI.transform.SetAsLastSibling();

                    BlackboxHandle.Of(this).Exert(_relicInfoPanelUI, "RelicsUI 열기");
                    _relicInfoPanelUI.Open();
                };
            }
            if (_dialogueUI)
                ((IInputController)_dialogueUI).Initialize(InputHub);


            foreach (var controlObj in _additionalInputControllers.Concat(_additionalInputControllables))
            {
                if (controlObj is IInputControllable controllable)
                {
                    BlackboxHandle.Of(this).Exert(controllable, $"등록: {controlObj.name}");
                    InputHub.Register(controllable);
                }
                if (controlObj is IInputController controller)
                {
                    BlackboxHandle.Of(this).Exert(controller, $"초기화: {controlObj.name}");
                    controller.Initialize(InputHub);
                }
            }
            #endregion
        }

        public void Register(IPlayer player, bool connectUI = true)
        {
            if (_isDestroyed)
                return;

            using var _ = BlackboxHandle.Of(this).ExertScope(PlayerManager, "PlayerManager에 Player 등록");

            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "UIManager에 PlayerUI 등록");

                var vm = new PlayerVM(player);
                var ui = _playerUI;
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

#if BLACKBOX
        private bool _logExported = false;
        private void Update()
        {
            if (!_logExported
                && Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.RightAlt))
            {
                _logExported = true;
                BlackboxHandle.Of(this).Export(openLogOption: OpenLogOption.Open);
            }
        }
#endif

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;

            if (SpawnManager.Instance)
            {
                BlackboxHandle.Of(this).Exert(SpawnManager.Instance, "Clear");
                SpawnManager.Instance.Clear();
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
