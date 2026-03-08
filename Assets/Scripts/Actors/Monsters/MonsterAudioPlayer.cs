using UnityEngine;
using System;
using Sound;
using Infrastructure;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    public class MonsterAudioPlayer : AudioSourceController,
        IInjectable<GameServices>
    {
        [SerializeField] private bool _overrideSpacialBlend = false;
        [SerializeField, Range(0, 1)] private float _spacialBlend = 0.7f;
        [SerializeField] private string _dieClipName = "Die";

        private IMonsterInternal _monster;
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            _gameServices = gameServices;
            SetVolume(_gameServices.SfxVolume);
        }

        private void Start()
        {
            if (!TryGetComponent(out _monster))
                throw new InvalidOperationException(Ctx(
                    $"{nameof(IMonsterInternal)} 컴포넌트를 가져오는 데 실패했습니다."));

            SpatialBlend = _overrideSpacialBlend
                ? _spacialBlend
                : _monster.Configuration.SfxSpatialBlend;

            _monster.ConditionChanged += conditionData =>
            {
                if (conditionData.Is(MonsterCondition.Attack))
                {
                    if (conditionData.Payload is not MonsterAttackData attackData)
                        return;

                    TryPlay(attackData.Name);
                }

                if (conditionData.Is(MonsterCondition.Dying))
                {
                    if (!TryPlay(_dieClipName, false))
                        Debug.LogWarning(
                            $"몬스터가 사망하였지만 '{_dieClipName}' 오디오를 재생하지 못했습니다.",
                            this);
                }
            };
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

                if (target._overrideSpacialBlend)
                    DrawDefaultInspector();
                else
                    DrawPropertiesExcluding(serializedObject, nameof(target._spacialBlend));

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
