using Actors;
using UI;
using UnityEngine;

namespace Game.Stage
{
    [RequireComponent(typeof(UIManager), typeof(PlayerManager), typeof(MonsterManager))]
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private UILibrary _uILibrary;
        [Space]
        [Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 플레이어를 자동으로 등록합니다")]
        [SerializeField] private bool _autoBindScenePlayer = true;
        [Tooltip("게임이 시작될 때 씬에 존재하는 Active 상태의 몬스터들을 자동으로 등록합니다")]
        [SerializeField] private bool _autoBindSceneMonsters = true;

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

            if (SpawnManaer.Instance != null)
                SpawnManaer.Instance.OnMonsterCreate(monster => Register(monster));
        }
        
        public void Register(IPlayer player, bool connectUI = true)
        {
            if (!PlayerManager.Register(player))
                return;

            if (connectUI)
            {
                var vm = new PlayerVM(player);
                var ui = UILibrary.PlayerUI;
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
