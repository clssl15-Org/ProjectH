using System;
using System.Linq;
using Infrastructure;
using UnityEngine;
using static Actors.Monsters.IndicatorConfiguration;

namespace Actors.Monsters
{
    public class IndicatorConfigurationView
    {
        public bool ShowPlayerDetection => Resolve(conf => conf.ShowPlayerDetection);
        public float PlayerDetectionHeight => Resolve(conf => conf.PlayerDetectionHeight);
        public float PlayerDetectionSize => Resolve(conf => conf.PlayerDetectionSize);
        public float PlayerDetectionShowTime => Resolve(conf => conf.PlayerDetectionShowTime);

        public bool ShowExclamationMark => Resolve(conf => conf.ShowExclamationMark);
        public float ExclamationMarkHeight => Resolve(conf => conf.ExclamationMarkHeight);
        public float ExclamationMarkSize => Resolve(conf => conf.ExclamationMarkSize);
        public float ExclamationMarkShowTime => Resolve(conf => conf.ExclamationMarkShowTime);

        public bool ShowDamageText => Resolve(conf => conf.ShowDamageText);
        public float DamageHeight => Resolve(conf => conf.DamageHeight);
        public float DamageFontSize => Resolve(conf => conf.DamageFontSize);
        public float DamageShowTime => Resolve(conf => conf.DamageShowTime);

        public Color DefaultDamageColor => Resolve(conf => conf.DefaultDamageColor);
        public DamageColorItem[] DamageColorItems => Resolve(conf => conf.DamageColorItems);


        private IndicatorConfiguration[] _configs;

        public IndicatorConfigurationView(params IndicatorConfiguration[] configs)
        {
            _configs = configs?.Where(c => c != null).ToArray();

            if (configs == null || configs.Length == 0)
                throw new ArgumentException(
                    $"[{nameof(IndicatorConfigurationView)}] 입력 인자는 null이 아닌 요소를 하나 이상 가지고 있어야 합니다.",
                    nameof(configs));
        }

        private T Resolve<T>(Func<IndicatorConfiguration, OverridableField<T>> selector)
        {
            var field = selector(_configs[0]);
            for (int i = 1; i < _configs.Length; i++)
                field = field.OverrideWith(selector(_configs[i]));

            return field.Value;
        }

    }
}
