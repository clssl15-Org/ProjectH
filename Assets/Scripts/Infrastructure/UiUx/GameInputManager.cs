using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.UiUx
{
    [System.Obsolete("¹Ì¿Ï¼º")]
    public class GameInputManager : MonoBehaviour
    {
        private GameObject _lastSelected;

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject)
                _lastSelected = EventSystem.current.currentSelectedGameObject;

            if (IsNavigationInput() && !EventSystem.current.currentSelectedGameObject)
            {
                if (_lastSelected != null && _lastSelected.activeInHierarchy)
                    EventSystem.current.SetSelectedGameObject(_lastSelected);
            }
        }

        private bool IsNavigationInput()
        {
            return Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
                   || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f
                   || Input.GetButtonDown("Submit")
                   || Input.GetButtonDown("Cancel");
        }
    }

}
