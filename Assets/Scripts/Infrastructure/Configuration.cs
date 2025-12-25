using Actors.Monsters;
using UnityEngine;

namespace Infrastructure
{
    [DisallowMultipleComponent]
    public class Configuration : MonoBehaviour
    {
        [field: Header("Rendering")]
        [field: SerializeField, Min(0f)] public float PixelScaleFactor { get; set; } = 0.03f;

        [field: Header("Indicator")]
        [field: SerializeField] public IndicatorConfiguration IndicatorConfiguration { get; set; }
    }
}
