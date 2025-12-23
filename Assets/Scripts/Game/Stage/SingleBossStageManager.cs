using System;
using UnityEngine;
using Actors;
using UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Stage
{
    public class SingleBossStageManager : StageManager
    {
        // Bindings
        [Space]
        [SerializeField] private GameObject _playerObject;
        [SerializeField] private GameObject _bossObject;
        [SerializeField] private BossUI _bossUI;

        // Internal
        private IBoss _boss;
        private bool _commenced = false;


        // Front
        protected override void Awake()
        {
            base.Awake();

            if (!_bossObject)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_bossObject)}이(가) 유효하지 않습니다.'"));

            if (!_bossObject.TryGetComponent(out _boss))
                throw new InvalidOperationException(
                    Ctx($"{nameof(_bossObject)}이(가) {nameof(IBoss)} 컴포넌트를 가지고 있지 않습니다.'"));

            if (!_bossUI)
                throw new InvalidOperationException(
                    Ctx($"{nameof(_bossUI)}이(가) 유효하지 않습니다.'"));
        }

        protected override void Start()
        {
            base.Start();
            _bossUI.SetToDisabled();
        }

        public void RegisterBoss(IBoss boss, bool createUI = true)
        {
            if (!MonsterManager.Register(boss))
                return;

            if (boss is IPlayerIInitializable playerIInitializable)
            {
                if (_playerObject
                    && _playerObject.TryGetComponent<IPlayer>(out var player))
                    playerIInitializable.InitializePlayer(player);
                else
                    Debug.LogWarning(Ctx(
                        $"{nameof(boss)}에 {nameof(_playerObject)}을(를) 등록하지 못했습니다. " +
                        $"해당 컴포넌트가 유효한지 확인하세요."));
            }

            if (createUI)
            {
                var vm = new MonsterVM(boss);
                _bossUI.Connect(vm);

                UIManager.RegisterVM(vm);
                UIManager.RegisterView(_bossUI);
            }
        }

        public void Commence()
        {
            if (_commenced) return;
            _commenced = true;

            RegisterBoss(_boss);

            _bossUI.Enable();
            _boss.Commence();
        }

        // Debug
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                Commence();
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
