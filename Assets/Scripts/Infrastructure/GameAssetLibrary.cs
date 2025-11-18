using UnityEngine;

namespace Infrastructure
{
    public class GameAssetLibrary : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material _solidColor;

        public Material SolidColor => Instantiate(_solidColor);
    }
}
