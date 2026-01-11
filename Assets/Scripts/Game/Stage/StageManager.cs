using Actors;
using UI;
using UnityEngine;
using BlackboxSystem;

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


        // Front
        protected virtual void Awake()
        {
            UIManager = GetComponent<UIManager>();
            PlayerManager = GetComponent<PlayerManager>();
            MonsterManager = GetComponent<MonsterManager>();
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
                        throw new System.InvalidOperationException(
                            $"[{nameof(StageManager)}] {nameof(_playerObject)}이(가) {nameof(IPlayer)} 컴포넌트를 가지고 있지 않습니다.");

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

            if (SpawnManaer.Instance)
                SpawnManaer.Instance.OnMonsterCreate(monster => Register(monster));
            else
                Debug.LogWarning(
                    "[StageManager] SpawnManaer.Instance이(가) 유효하지 않습니다. " +
                    "새로 스폰되는 몬스터는 매니저에 등록되지 않으며, UI 등이 생성되지 않을 수 있습니다.",
                    this);
        }
        
        public void Register(IPlayer player, bool connectUI = true)
        {
            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                var vm = new PlayerVM(player);
                var ui = _playerUI;
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui);
            }
        }

        public void Register(IMonster monster, bool createUI = true)
        {
            if (!MonsterManager.Register(monster))
                return;

            if (createUI)
            {
                var vm = new MonsterVM(monster);
                var ui = UILibrary.HealthBar;
                ui.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(ui);
            }
        }

        private void OnDestroy()
        {
            UIManager.Destroy();
            PlayerManager.Destroy();
            MonsterManager.Destroy();
        }
    }
}
