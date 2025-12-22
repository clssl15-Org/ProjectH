using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.UiUx
{
    public class AutoDeselect : MonoBehaviour, IPointerUpHandler
    {
        [SerializeField] private bool _enabled = true;

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_enabled) return;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
