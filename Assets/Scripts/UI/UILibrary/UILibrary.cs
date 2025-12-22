using UnityEngine;

namespace UI
{
    public class UILibrary : MonoBehaviour
    {
        [SerializeField] private GameObject _playerUI;
        [SerializeField] private GameObject _healthBar;

        public PlayerUI PlayerUI => _playerUI?.GetComponent<PlayerUI>();
        public HealthBar HealthBar => Instantiate(_healthBar).GetComponent<HealthBar>();


        private void Start()
        {
            if (!_playerUI)
                Debug.LogError(
                    Ctx($"{nameof(_playerUI)} 필드가 할당되지 않았습니다."),this);

            if (!_healthBar)
                Debug.LogError(
                    Ctx($"{nameof(_healthBar)} 필드가 할당되지 않았습니다."), this);
        }

        private string Ctx(string message) => $"[{nameof(UILibrary)}] {message}";
    }
}
