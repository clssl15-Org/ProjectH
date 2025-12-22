using Actors;
using UI;
using UnityEngine;

namespace Game.Stage
{
    [RequireComponent(typeof(UIManager), typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private UILibrary _uiLibrary;
        [Space]
        [Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [SerializeField] private bool _autoBindScenePlayer = true;
        [Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [SerializeField] private bool _autoBindSceneMonsters = true;

        // Internal
        private UIManager _uiManager;
        private PlayerManager _playerManager;
        private MonsterManager _monsterManager;


        // Front
        private void Awake()
        {
            _uiManager = GetComponent<UIManager>();
            _playerManager = GetComponent<PlayerManager>();
            _monsterManager = GetComponent<MonsterManager>();
        }

        private void Start()
        {
            if (_autoBindScenePlayer)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null && playerObject.TryGetComponent<IPlayer>(out var player))
                    Register(player);
            }

            if (_autoBindSceneMonsters)
                foreach (var monsterObject in GameObject.FindGameObjectsWithTag("Monster"))
                {
                    if (!monsterObject.TryGetComponent<IMonster>(out var monster))
                        continue;

                    Register(monster);
                }
        }
        
        public void Register(IPlayer player, bool connectUI = true)
        {
            if (!_playerManager.Register(player))
                return;

            if (connectUI)
            {
                var vm = new PlayerVM(player);
                var ui = _uiLibrary.PlayerUI;
                ui.Connect(vm);

                _uiManager.RegisterVM(vm);
                _uiManager.RegisterView(ui);
            }
        }

        public void Register(IMonster monster, bool createUI = true)
        {
            if (!_monsterManager.Register(monster))
                return;

            if (createUI)
            {
                var vm = new MonsterVM(monster);
                var ui = _uiLibrary.HealthBar;
                ui.Connect(vm);

                _uiManager.RegisterVM(vm);
                _uiManager.RegisterView(ui);
            }
        }

        private void OnDestroy()
        {
            _uiManager.Destroy();
            _playerManager.Destroy();
            _monsterManager.Destroy();
        }
    }
}
