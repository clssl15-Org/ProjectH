using System;
using System.Linq;
using Sound;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    public class MonsterAudioPlayer : SfxAudioController
    {
        [field: Header("Monster Audio Player")]
        [field: SerializeField] public bool StandaloneMode { get; set; } = false;
        [Space]
        [SerializeField] private AttackEvent _defaultHitSoundPlayTiming = AttackEvent.Started;
        [Serializable]
        public struct HitSoundPlayTimingOptions
        {
            public string Name;
            public AttackEvent Timing;
        }
        [SerializeField] private HitSoundPlayTimingOptions[] _hitSoundPlayTimings;

        [SerializeField] private string _dieClipName = "Die";
        [Space]
        [SerializeField] private bool _overrideSpatialBlend = false;
        [SerializeField, Range(0, 1)] private float _spatialBlend = 0.7f;

        private IMonsterInternal _monster;

        private void Start()
        {
            ApplySettings();
            object playToken = null;

            if (!StandaloneMode)
            {
                if (!TryGetComponent(out _monster))
                    throw new InvalidOperationException(Ctx(
                        $"{nameof(IMonsterInternal)} 컴포넌트를 가져오는 데 실패했습니다."));

                _monster.ConditionChanged += conditionData =>
                {
                    if (_defaultHitSoundPlayTiming != AttackEvent.None)
                    {
                        if (conditionData.Is(MonsterCondition.Attack, MonsterCondition.Heal))
                        {
                            if (conditionData.Payload is not MonsterAttackData attackData)
                                return;

                            var targetEventType = _defaultHitSoundPlayTiming;
                            var targetOption = _hitSoundPlayTimings.FirstOrDefault(t => t.Name == attackData.Name);
                            if (!string.IsNullOrEmpty(targetOption.Name)) targetEventType = targetOption.Timing;

                            var token = playToken = new();
                            attackData.EventOccurred += attackEvent =>
                            {
                                if (attackEvent == targetEventType)
                                    TryPlay(attackData.Name);
                            };

                            if (TryGetAudioData(attackData.Name, out var clip)
                                && clip.PlayOption == PlayOption.Loop)
                            {
                                attackData.EventOccurred += attackEvent =>
                                {
                                    if (playToken == token && attackEvent == AttackEvent.Finished)
                                    {
                                        Stop();

                                        var finalizerName = attackData.Name + "_Finish";
                                        if (TryGetAudioData(finalizerName, out var clip))
                                            TryPlay(finalizerName);
                                    }
                                };
                            }
                        }
                    }

                    if (conditionData.Is(MonsterCondition.Dying))
                    {
                        if (!TryPlay(_dieClipName))
                            Debug.LogWarning(
                                Ctx($"몬스터가 사망하였지만 '{_dieClipName}' 오디오를 재생하지 못했습니다."),
                                this);
                    }
                };
            }
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
