using UnityEngine;

namespace Game.Title
{
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] private Home _home;
        [SerializeField] private Menu _menu;

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
