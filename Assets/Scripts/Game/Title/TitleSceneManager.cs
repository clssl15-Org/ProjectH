using Infrastructure;
using Sound;
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
        [SerializeField] private GuideAndWorldRecordsUI _guideUI;

        private void Awake()
        {
            var inputHub = GetComponent<InputHub>();

            inputHub.Add(_settingsUI.GetOpenerSubject());
            inputHub.Add(_menu);

            ((IInputLayerController)_settingsUI).Initialize(inputHub);
            ((IInputLayerController)_guideUI).Initialize(inputHub);

            _settingsUI.OpenGuideUI += _guideUI.Open;
            _menu.OpenGuide += _guideUI.Open;
        }

        private void Start()
        {
            _home.SetToEnabled();
            _menu.SetToDisabled();

            _home.Enabling += _menu.Disable;
            _home.Disabling += _menu.Enable;

            _menu.Enabling += _home.Disable;
            _menu.Disabling += _home.Enable;

            BgmPlayManager.Play(BgmName.Title);
        }

        private void OnDestroy()
        {
            BgmPlayManager.Stop();
        }
    }
}
