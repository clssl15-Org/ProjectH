using System;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters
{
    [CreateAssetMenu(fileName = "Indicator Configuration", menuName = "Project H/Indicator/Indicator Configuration")]
    public class IndicatorConfiguration : ScriptableObject
    {
        [field: Header("Player Detection")]
        [field: SerializeField] public OverridableField<bool> ShowPlayerDetection { get; set; } = new(true);
        [field: SerializeField] public OverridableField<float> PlayerDetectionHeight { get; set; } = new(0.3f);
        [field: SerializeField] public OverridableField<float> PlayerDetectionSize { get; set; } = new(1f);
        [field: SerializeField] public OverridableField<float> PlayerDetectionShowTime { get; set; } = new(0.5f);

        [field: Header("Exclamation Mark")]
        [field: SerializeField] public OverridableField<bool> ShowExclamationMark { get; set; } = new(true);
        [field: SerializeField] public OverridableField<float> ExclamationMarkHeight { get; set; } = new(0.3f);
        [field: SerializeField] public OverridableField<float> ExclamationMarkSize { get; set; } = new(1f);
        [field: SerializeField] public OverridableField<float> ExclamationMarkShowTime { get; set; } = new(0.5f);

        [field: Header("Damage Text")]
        [field: SerializeField] public OverridableField<bool> ShowDamageText { get; set; } = new(true);
        [field: SerializeField] public OverridableField<float> DamageHeight { get; set; } = new(0.7f);
        [field: SerializeField] public OverridableField<float> DamageFontSize { get; set; } = new(5f);
        [field: SerializeField] public OverridableField<float> DamageShowTime { get; set; } = new(0.5f);

        [Serializable]
        public struct DamageColorItem
        {
            public int Damage;
            public Color Color;
        }

        [field: Space]
        [field: SerializeField]
        public OverridableField<Color> DefaultDamageColor { get; set; } = new(Color.white);

        [field: SerializeField]
        [field: Tooltip("대미지는 오름차순으로 정렬되어 있어야 합니다.")]
        public OverridableField<DamageColorItem[]> DamageColorItems { get; set; } = new(new[]
        {
            new DamageColorItem { Damage = 10, Color = new(255 / 255f, 219 / 255f, 126 / 255f) },
            new DamageColorItem { Damage = 30, Color = new(244 / 255f, 170 / 255f, 75 / 255f) },
        });
    }
}
