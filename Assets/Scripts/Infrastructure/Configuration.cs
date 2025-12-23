using UnityEngine;

namespace Infrastructure
{
    [DisallowMultipleComponent]
    public class Configuration : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _pixelScaleFactor = 1f;

        public float PixelScaleFactor => _pixelScaleFactor;
    }
}
