using Infrastructure;
using UI;
using UnityEngine;

namespace Game.Title
{
    [RequireComponent(typeof(InputHub))]
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] private Home _home;
        [SerializeField] private Menu _menu;
        [Space]
        [SerializeField] private SettingsUI _settingsUI;

        private void Awake()
        {
            var inputHub = GetComponent<InputHub>();

            inputHub.Register(_settingsUI);
            inputHub.Register(_menu);

            ((IInputController)_settingsUI).Initialize(inputHub);
        }

        private void Start()
        {
            _home.SetToEnabled();
            _menu.SetToDisabled();

            _home.Enabling += _menu.Disable;
            _home.Disabling += _menu.Enable;

            _menu.Enabling += _home.Disable;
            _menu.Disabling += _home.Enable;
        }
    }
}
