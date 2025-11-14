using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actors;
using UI;
using UI.Monsters;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIManager), typeof(MonsterManager))]
    public class SceneManager : MonoBehaviour
    {
        // Bindings
        [SerializeField] private UILibrary _uiLibrary;

        // Internal
        private UIManager _uiManager;
        private MonsterManager _monsterManager;

        // Front
        private void Awake()
        {
            _uiManager = GetComponent<UIManager>();
            _monsterManager = GetComponent<MonsterManager>();
        }

        public void Register(IMonster monster, bool createUI = true)
        {
            if (!_monsterManager.Register(monster))
                return;

            if (createUI)
            {
                _uiManager.RegisterModel(
                    new MonsterVM(monster, _uiLibrary, _uiManager.RegisterView));
            }
        }
    }
}
