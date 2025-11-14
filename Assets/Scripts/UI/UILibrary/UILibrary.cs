using UI.Views;
using UnityEngine;

namespace UI
{
    public class UILibrary : MonoBehaviour
    {
        [SerializeField] private GameObject _healthBar;

        public HealthBar HealthBar => Instantiate(_healthBar).GetComponent<HealthBar>();
    }
}
