using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure
{
    public class ScrollRectResetHandler : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;

        void OnEnable()
        {
            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
