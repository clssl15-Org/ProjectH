using Actors.Monsters;
using UnityEngine;

namespace Infrastructure
{
    [DisallowMultipleComponent]
    public class Configuration : MonoBehaviour
    {
        // WARNING: Never Change this value.
        // PixelScaleFactor must be '0.03f'
        public float PixelScaleFactor { get; } = 0.03f;
        [field: SerializeField, Range(0, 1)] public float SfxSpatialBlend { get; set; } = 0.7f;

        [field: Header("Indicator")]
        [field: SerializeField] public IndicatorConfiguration IndicatorConfiguration { get; set; }
    }
}
