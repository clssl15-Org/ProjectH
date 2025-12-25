using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(RectTransform))]
    public class IndicatorText : MonoBehaviour
    {
        private RectTransform _transform;

        private void Awake() =>
            _transform = GetComponent<RectTransform>();

        private void Update()
        {
            if (_transform.lossyScale.x < 0)
                _transform.localScale = new Vector3(-_transform.localScale.x, 1, 1);
        }
    }
}
