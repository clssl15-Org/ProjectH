using System;
using Infrastructure;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [DisallowMultipleComponent]
    public class IndicatorHub : MonoBehaviour
    {
        [field: SerializeField] public IndicatorConfiguration ConfigurationOverride { get; set; }

        private IMonsterInternal _monster;
        private IndicatorConfigurationView _config;


        private void Start()
        {
            if (!TryGetComponent(out _monster))
                throw new InvalidOperationException(Ctx(
                    $"{nameof(IMonsterInternal)} 컴포넌트를 가져오는 데 실패했습니다."));

            var configuraton = _monster.Configuration?.IndicatorConfiguration
                ?? throw new InvalidOperationException(Ctx(
                    $"{nameof(_monster.Configuration)}이(가) 유효하지 않습니다."));
            _config = new(configuraton, ConfigurationOverride);

            _monster.ConditionChanged += cd =>
            {
                if (cd.Is(MonsterCondition.PlayerDetected) && _config.ShowPlayerDetection)
                    ShowPlayerDetectionIndicator();

                if (cd.Is(MonsterCondition.Attack) && _config.ShowExclamationMark)
                {
                    if (cd.Payload is not bool isRanged)
                    {
                        Debug.LogWarning(
                            Ctx($"Attack 이벤트의 Payload는 bool 형식이어야 하지만 '{cd.Payload?.GetType().Name ?? "null"}'이(가) 감지되었습니다."),
                            this);
                        return;
                    }

                    if (!isRanged)
                        return;

                    ShowExclamationMarkIndicator();
                }

                if (cd.Is(MonsterCondition.Damaged) && _config.ShowDamageText)
                {
                    if (cd.Payload is not DamageInfo damageInfo)
                    {
                        Debug.LogWarning(
                            Ctx($"Damaged 이벤트의 Payload는 {nameof(DamageInfo)} 형식이어야 하지만 '{cd.Payload?.GetType().Name ?? "null"}'이(가) 감지되었습니다."),
                            this);
                        return;
                    }

                    ShowDamageTextIndicator(damageInfo.Damage);
                }
            };
        }

        private void ShowPlayerDetectionIndicator()
        {
            _monster
                .GameAssetsLibrary
                .Indicator_PlayerDetection
                .ShowAsIndicator(
                    _monster,
                    _config.PlayerDetectionHeight * _monster.transform.lossyScale.z,
                    _config.PlayerDetectionShowTime,
                    _config.PlayerDetectionSize);
        }

        private void ShowExclamationMarkIndicator()
        {
            _monster
                .GameAssetsLibrary
                .Indicator_ExclamationMark
                .ShowAsIndicator(
                    _monster,
                    _config.ExclamationMarkHeight * _monster.transform.lossyScale.z,
                    _config.ExclamationMarkShowTime,
                    _config.ExclamationMarkSize);
        }

        private void ShowDamageTextIndicator(int damage)
        {
            var text = _monster
                .GameAssetsLibrary
                .Indicator_Text
                .ShowAsIndicator(
                    _monster,
                    damage.ToString(),
                    _config.DamageHeight,
                    _config.DamageShowTime,
                    _config.DamageFontSize);


            text.color = _config.DefaultDamageColor;

            if (_config.DamageColorItems?.Length > 0)
                foreach (var dci in _config.DamageColorItems)
                {
                    if (dci.Damage > damage)
                        break;

                    text.color = dci.Color;
                }
        }

        private string Ctx(string message) => $"[{nameof(IndicatorHub)}] {name}: {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(IndicatorHub))]
        private class IndicatorHubEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space(8);

                if (Application.isPlaying)
                {
                    var target = (IndicatorHub)base.target;

                    if (GUILayout.Button("Show Player Detection Indicator"))
                        target.ShowPlayerDetectionIndicator();
                    if (GUILayout.Button("Show Exclamation Mark Indicator"))
                        target.ShowExclamationMarkIndicator();
                    if (GUILayout.Button("Show Damage Text Indicator"))
                        target.ShowDamageTextIndicator(1);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Indicator 테스트는 플레이 모드에서만 사용할 수 있습니다.",
                        MessageType.Info);
                }
            }
        }
#endif
    }
}
