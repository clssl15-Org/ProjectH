using System;
using Sound;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    public class MonsterAudioPlayer : SfxAudioController
    {
        [Header(nameof(MonsterAudioPlayer))]
        [SerializeField] private AttackPhase _defaultHitSoundPlayTiming = AttackPhase.Executing;
        [Serializable]
        public struct HitSoundPlayTimingOptions
        {
            public string Name;
            public AttackPhase Timing;
        }
        [SerializeField] private HitSoundPlayTimingOptions[] _hitSoundPlayTimings;

        [SerializeField] private string _dieClipName = "Die";
        [Space]
        [SerializeField] private bool _overrideSpatialBlend = false;
        [SerializeField, Range(0, 1)] private float _spatialBlend = 0.7f;

        private IMonsterInternal _monster;

        private void Start()
        {
            if (!TryGetComponent(out _monster))
                throw new InvalidOperationException(Ctx(
                    $"{nameof(IMonsterInternal)} 컴포넌트를 가져오는 데 실패했습니다."));

            ApplySettings();

            _monster.ConditionChanged += conditionData =>
            {
                if (_defaultHitSoundPlayTiming != AttackPhase.None
                    && conditionData.Is(MonsterCondition.Attack, MonsterCondition.Heal))
                {
                    if (conditionData.Payload is not MonsterAttackData attackData)
                        return;

                    var timing = _defaultHitSoundPlayTiming;
                    var target = _hitSoundPlayTimings.FirstOrDefault(t => t.Name == attackData.Name);
                    if (!string.IsNullOrEmpty(target.Name)) timing = target.Timing;

                    switch (timing)
                    {
                        case AttackPhase.Executing:
                            attackData.Executing += () => TryPlay(attackData.Name);
                            break;

                        case AttackPhase.HitPlayer:
                            attackData.HitPlayer += () => TryPlay(attackData.Name);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(_defaultHitSoundPlayTiming),
                                Ctx($"알 수 없는 공격 재생 타이밍 '{_defaultHitSoundPlayTiming}'이(가) 입력되었습니다."));
                    }
                }

                if (conditionData.Is(MonsterCondition.Dying))
                {
                    if (!TryPlay(_dieClipName, false))
                        Debug.LogWarning(
                            $"몬스터가 사망하였지만 '{_dieClipName}' 오디오를 재생하지 못했습니다.",
                            this);
                }
            };

            _monster.ConditionChanged += conditionData =>
            {
                if (conditionData.Is(MonsterCondition.Dying))
                {
                    if (!TryPlay(_dieClipName, false))
                        Debug.LogWarning(
                            $"몬스터가 사망하였지만 '{_dieClipName}' 오디오를 재생하지 못했습니다.",
                            this);
                }
            };
        }

        protected override void ApplySettings()
        {
            base.ApplySettings();

            if (_overrideSpatialBlend) SpatialBlend = _spatialBlend;
            else if (_monster != null) SpatialBlend = _monster.Configuration.SfxSpatialBlend;
        }

        private string Ctx(string message) => $"[{nameof(MonsterAudioPlayer)}: {name}] {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(MonsterAudioPlayer)), CanEditMultipleObjects]
        protected class MonsterAudioPlayerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var target = (MonsterAudioPlayer)base.target;

                if (target._overrideSpatialBlend)
                    DrawDefaultInspector();
                else
                    DrawPropertiesExcluding(serializedObject, nameof(target._spatialBlend));

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
