using System;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
using BlackboxSystem;
using Sound;
using UnityEngine;

namespace UI.PlayerView
{
    [RequireComponent(typeof(SfxAudioController))]
    internal class SkillSelectionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _iconGroup;

        [Header("Settings")]
        [SerializeField] private float _iconWidth = 100f;   // 아이콘 간격 (이동 단위)
        [SerializeField] private float _moveSpeed = 10f;    // 이동 속도 (Lerp Speed)

        [Serializable]
        private struct IconInfo
        {
            public SkillType SkillType;
            public GameObject IconPrefab;
        }
        [SerializeField] private IconInfo[] _iconConfigs;

        // [수정] Inspector 설정용(IconInfo)과 런타임 생성용(RuntimeIcon)을 분리하여 관리
        private class RuntimeIcon
        {
            public SkillType SkillType;
            public RectTransform RectTransform;
        }

        private readonly List<RuntimeIcon> _icons = new();
        private Dictionary<SkillType, GameObject> _configMap;

        private int _targetIndex = 0;       // 목표 인덱스
        private float _targetPosX = 0f;     // 목표 X 좌표
        private bool _needsTeleport = false; // 이동 완료 후 순간이동 필요 여부

        private SfxAudioController _sfxAudioController;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _sfxAudioController = GetComponent<SfxAudioController>();

            // O(1) 검색을 위한 캐싱
            _configMap = _iconConfigs.ToDictionary(x => x.SkillType, x => x.IconPrefab);
        }

        private void Update()
        {
            if (Mathf.Abs(_iconGroup.anchoredPosition.x - _targetPosX) > 0.1f)
            {
                float newX = Mathf.Lerp(_iconGroup.anchoredPosition.x, _targetPosX, Time.deltaTime * _moveSpeed);
                _iconGroup.anchoredPosition = new Vector2(newX, _iconGroup.anchoredPosition.y);
            }
            else
            {
                _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);

                if (_needsTeleport)
                {
                    _needsTeleport = false;
                    ForceResetToStartIndex();
                }
            }
        }

        public void Initialize(SkillType[] skills)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize: [{string.Join(", ", skills)}]");

            // 1. 기존 정리
            for (int i = _iconGroup.childCount - 1; i >= 0; i--)
                Destroy(_iconGroup.GetChild(i).gameObject);
            _icons.Clear();

            if (skills.Length == 0) return;

            // [수정] LINQ 지연 평가 오류를 막기 위해 명시적 List 복사 후 더미 추가
            List<SkillType> skillsToCreate = skills.ToList();
            if (skillsToCreate.Count > 1)
            {
                skillsToCreate.Add(skillsToCreate[0]);
            }

            foreach (var skill in skillsToCreate)
            {
                if (!_configMap.TryGetValue(skill, out var prefab) || prefab == null)
                {
                    // [핵심] Inspector에 프리팹이 등록되지 않아 조용히 무시되는 버그를 잡기 위한 에러 로그
                    Debug.LogError($"[SkillSelectionManager] {skill} 프리팹이 _iconConfigs에 누락되었습니다! UI에 표시되지 않습니다.");
                    continue;
                }

                var instance = Instantiate(prefab, _iconGroup, false);
                instance.SetActive(true);

                _icons.Add(new RuntimeIcon { SkillType = skill, RectTransform = instance.GetComponent<RectTransform>() });
            }

            // 3. 아이콘들을 가로로 쭉 배치
            ArrangeIconsHorizontally();

            // 초기 상태 설정
            _targetIndex = 0;
            _targetPosX = 0;
            _iconGroup.anchoredPosition = Vector2.zero;
        }

        private void ArrangeIconsHorizontally()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                // [수정] GetComponent 오버헤드 제거 (RuntimeIcon 생성 시 미리 캐싱함)
                _icons[i].RectTransform.anchoredPosition = new Vector2(i * _iconWidth, 0);
            }
        }

        public void OnSkillChanged(SkillType targetSkill)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Change Skill Icon: {targetSkill}");

            int dummyIndex = _icons.Count - 1;
            if (_targetIndex == dummyIndex)
            {
                _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);
                ForceResetToStartIndex();
            }

            int targetIndex = FindSmartTargetIndex(targetSkill);

            if (targetIndex == -1 || targetIndex == _targetIndex) return;

            _targetIndex = targetIndex;
            _targetPosX = -1 * (_targetIndex * _iconWidth);
            _needsTeleport = (targetIndex == dummyIndex);

            _sfxAudioController.Play("PlayerSkillChange", AudioSourceController.PlayOption.Independently);
            BlackboxHandle.Of(this).Write($"Moving to index {_targetIndex} (TargetX: {_targetPosX})");
        }

        public void AddSkill(SkillType targetSkill)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Add Skill: {targetSkill}");

            var newIcon = CreateRuntimeIconInstance(targetSkill);

            if (_icons.Count == 0)
            {
                _icons.Add(newIcon);
            }
            else if (_icons.Count == 1)
            {
                var firstSkillType = _icons[0].SkillType;
                var dummyIcon = CreateRuntimeIconInstance(firstSkillType);

                _icons.Add(newIcon);
                _icons.Add(dummyIcon);
            }
            else
            {
                int insertIndex = _icons.Count - 1;
                _icons.Insert(insertIndex, newIcon);
                newIcon.RectTransform.SetSiblingIndex(insertIndex);

                // [수정] 런타임에 스킬 추가 시, 현재 타겟이 더미 쪽에 있었다면 타겟 인덱스도 밀어주어야 위치가 튀지 않음
                if (_targetIndex >= insertIndex)
                {
                    _targetIndex++;
                    _targetPosX = -1 * (_targetIndex * _iconWidth);
                    _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);
                }
            }

            ArrangeIconsHorizontally();
            BlackboxHandle.Of(this).Write($"Skill Added. Total Count: {_icons.Count}");
        }

        private RuntimeIcon CreateRuntimeIconInstance(SkillType skill)
        {
            if (!_configMap.TryGetValue(skill, out var prefab) || prefab == null)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"[SkillManager] Prefab not found for {skill}. Inspector를 확인하세요."));
            }

            var instance = Instantiate(prefab, _iconGroup, false);
            instance.SetActive(true);

            return new RuntimeIcon { SkillType = skill, RectTransform = instance.GetComponent<RectTransform>() };
        }

        private int FindSmartTargetIndex(SkillType targetSkill)
        {
            // [수정] 역방향으로 튕기는 것을 막기 위해 '현재 인덱스 이후'에서 먼저 탐색하여 정방향 진행 유도
            int forwardIndex = _icons.FindIndex(_targetIndex, x => x.SkillType == targetSkill);

            // 현재 위치 이후에 없다면 처음부터 다시 탐색
            int basicIndex = forwardIndex != -1 ? forwardIndex : _icons.FindIndex(x => x.SkillType == targetSkill);

            if (basicIndex == -1) return -1;

            int lastRealIndex = _icons.Count - 2;
            int dummyIndex = _icons.Count - 1;

            if (_targetIndex == lastRealIndex && basicIndex == 0)
            {
                return dummyIndex;
            }

            return basicIndex;
        }

        private void ForceResetToStartIndex()
        {
            _targetIndex = 0;
            _targetPosX = 0;

            _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);

            BlackboxHandle.Of(this).Write("Teleported logic applied (Infinite Scroll)");
        }
    }
}
