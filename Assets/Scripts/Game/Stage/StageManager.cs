using Actors;
using BlackboxSystem;
using UI;
using UnityEngine;

namespace Game.Stage
{
    [RequireComponent(typeof(UIManager), typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour
    {        
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindScenePlayer { get; set; } = false;
        [field: Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [field: SerializeField] protected bool AutoBindSceneMonsters { get; set; } = true;

        [Space]
        [SerializeField] private UILibrary _uILibrary;

        [Header("Player")]
        [SerializeField] private GameObject _playerObject;
        [SerializeField] private PlayerUI _playerUI;

        protected IPlayer Player { get; private set; }
        protected UILibrary UILibrary => _uILibrary;

        protected UIManager UIManager { get; private set; }
        protected PlayerManager PlayerManager { get; private set; }
        protected MonsterManager MonsterManager { get; private set; }

        private bool _isDestroyed = false;


        // Front
        protected virtual void Awake()
        {
            UIManager = GetComponent<UIManager>();
            PlayerManager = GetComponent<PlayerManager>();
            MonsterManager = GetComponent<MonsterManager>();

            BlackboxHandle.Initialize(Application.persistentDataPath, Debug.Log);
            BlackboxHandle.Of(this).Write("Awake");
        }

        protected virtual void Start()
        {
            if (AutoBindScenePlayer)
            {
                foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (playerObj
                        && playerObj.activeSelf
                        && playerObj.TryGetComponent<IPlayer>(out var player))
                    {
                        Player = player;
                        Register(player);

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
                                $"Start: [{nameof(StageManager)}] {nameof(_playerObject)}이(가) {nameof(IPlayer)} 컴포넌트를 가지고 있지 않습니다."));
                    }

                    Player = player;
                    Register(Player);
                }
            }

            if (AutoBindSceneMonsters)
                foreach (var monsterObject in GameObject.FindGameObjectsWithTag("Monster"))
                {
                    if (!monsterObject.TryGetComponent<IMonster>(out var monster))
                        continue;

                    Register(monster);
                }

            if (SpawnManager.Instance)
            {
                BlackboxHandle.Of(this).Exert(SpawnManager.Instance, "Start: Spawner에 Register 대리자 등록");
                SpawnManager.Instance.OnMonsterCreate(monster => Register(monster));
            }
            else
                Debug.LogWarning(BlackboxHandle.Of(this).Write(
                    "[StageManager] SpawnManager.Instance이(가) 유효하지 않습니다. " +
                    "새로 스폰되는 몬스터는 매니저에 등록되지 않으며, UI 등이 생성되지 않을 수 있습니다."),
                    this);
        }
        
        public void Register(IPlayer player, bool connectUI = true)
        {
            if (_isDestroyed)
                return;

            BlackboxHandle.Of(this).Exert(player, "Register: PlayerManager에 Player 등록");

            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "Register: UIManager에 PlayerUI 등록");

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

            BlackboxHandle.Of(this).Exert(monster, "Register: MonsterManager에 Monster 등록");

            if (!MonsterManager.Register(monster))
                return;

            if (createUI)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "Register: UIManager에 MonsterUI 등록");

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
                && Input.GetKey(KeyCode.RightControl))
            {
                _logExported = true;
                BlackboxHandle.Of(this).Export(openLog: true);
            }
        }
#endif

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;

            if (SpawnManager.Instance)
            {
                BlackboxHandle.Of(this).Exert(SpawnManager.Instance, "OnDestroy: Clear");
                SpawnManager.Instance.Clear();
            }

            if (UIManager)
            {
                BlackboxHandle.Of(this).Exert(UIManager, "OnDestroy: Destroy");
                UIManager.Destroy();
            }

            if (PlayerManager)
            {
                BlackboxHandle.Of(this).Exert(PlayerManager, "OnDestroy: Destroy");
                PlayerManager.Destroy();
            }

            if (MonsterManager)
            {
                BlackboxHandle.Of(this).Exert(MonsterManager, "OnDestroy: Destroy");
                MonsterManager.Destroy();
            }
        }
    }
}
