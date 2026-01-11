using UnityEngine;

namespace UI
{
    public class UILibrary : MonoBehaviour
    {
        [SerializeField] private GameObject _healthBar;

        public HealthBarUI HealthBar => Instantiate(_healthBar).GetComponent<HealthBarUI>();


        private void Start()
        {
            if (!_healthBar)
                Debug.LogError(
                    Ctx($"{nameof(_healthBar)} 필드가 할당되지 않았습니다."), this);
        }

        private string Ctx(string message) => $"[{nameof(UILibrary)}] {message}";
    }
}
