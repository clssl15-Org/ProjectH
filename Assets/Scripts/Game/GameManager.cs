using Actors;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SceneManager))]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _monsters;

        private SceneManager _sceneManager;


        private void Awake()
        {
            _sceneManager = GetComponent<SceneManager>();
        }

        private void Start()
        {
            foreach (var monster in _monsters)
            {
                if (!monster.TryGetComponent<IMonster>(out var monsterScript))
                    continue;

                _sceneManager.Register(monsterScript);
            }
        }
    }
}
