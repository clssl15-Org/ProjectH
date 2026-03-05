using System;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
using BlackboxSystem;
using UnityEngine;

namespace UI.PlayerView
{
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

        private readonly List<IconInfo> _icons = new();
        private Dictionary<SkillType, GameObject> _configMap;
        
        private int _targetIndex = 0;       // 목표 인덱스
        private float _targetPosX = 0f;     // 목표 X 좌표
        private bool _needsTeleport = false; // 이동 완료 후 순간이동 필요 여부

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            // O(1) 검색을 위한 캐싱
            _configMap = _iconConfigs.ToDictionary(x => x.SkillType, x => x.IconPrefab);
        }

        private void Update()
        {
            // 현재 위치와 목표 위치가 다르면 부드럽게 이동 (코루틴 대체)
            if (Mathf.Abs(_iconGroup.anchoredPosition.x - _targetPosX) > 0.1f)
            {
                float newX = Mathf.Lerp(_iconGroup.anchoredPosition.x, _targetPosX, Time.deltaTime * _moveSpeed);
                _iconGroup.anchoredPosition = new Vector2(newX, _iconGroup.anchoredPosition.y);
            }
            else
            {
                // 목표 지점 거의 도착 시
                _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);

                // [무한 스크롤 핵심] 더미(A')에 도착했다면? -> 진짜(A) 위치로 순간이동
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

            // 2. 더미 데이터 포함하여 생성 (A, B, C -> A, B, C, A')
            IEnumerable<SkillType> skillsToCreate = skills.Length > 1 
                ? skills.Append(skills[0]) 
                : skills;

            foreach (var skill in skillsToCreate)
            {
                if (!_configMap.TryGetValue(skill, out var prefab) || !prefab) continue;

                var instance = Instantiate(prefab, _iconGroup, false);
                instance.SetActive(true);

                _icons.Add(new IconInfo { SkillType = skill, IconPrefab = instance }); // Prefab 필드에 인스턴스 저장 (편의상)
            }

            // 3. 아이콘들을 가로로 쭉 배치 (Horizontal Layout Group 대신 수동 배치 추천)
            ArrangeIconsHorizontally();

            // 초기 상태 설정
            _targetIndex = 0;
            _targetPosX = 0;
            _iconGroup.anchoredPosition = Vector2.zero;
        }

        // 아이콘들을 _iconWidth 간격으로 가로 배치
        private void ArrangeIconsHorizontally()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                var rect = _icons[i].IconPrefab.GetComponent<RectTransform>();
                // 인덱스가 커질수록 오른쪽(+)으로 배치
                rect.anchoredPosition = new Vector2(i * _iconWidth, 0);
            }
        }

        public void OnSkillChanged(SkillType targetSkill)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Change Skill Icon: {targetSkill}");

            int dummyIndex = _icons.Count - 1;
            if (_targetIndex == dummyIndex)
            {
                // 현재 더미(A')를 향해 가고 있었다면?
                // 1. 시각적으로 즉시 더미 위치로 이동 (애니메이션 스킵)
                _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);

                // 2. 논리적으로 0번(A) 위치로 리셋 (A'와 A는 모습이 같으므로 티가 안 남)
                ForceResetToStartIndex();

                BlackboxHandle.Of(this).Write("Input received during loop. Forced reset to start.");
            }

            // 1. 목표 인덱스 계산
            int targetIndex = FindSmartTargetIndex(targetSkill);

            // 현재 위치와 같거나, 찾을 수 없으면 무시
            if (targetIndex == -1 || targetIndex == _targetIndex) return;

            // 2. 이동 목표 설정
            _targetIndex = targetIndex;
            _targetPosX = -1 * (_targetIndex * _iconWidth);

            // 3. 만약 목표가 '더미(마지막)'라면 이동 후 텔레포트 예약
            _needsTeleport = (targetIndex == dummyIndex);

            BlackboxHandle.Of(this).Write($"Moving to index {_targetIndex} (TargetX: {_targetPosX})");
        }

        public void AddSkill(SkillType targetSkill)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Add Skill: {targetSkill}");

            // 1. 아이콘 생성
            var newIcon = CreateIconInstance(targetSkill);

            // 2. 리스트 상태에 따른 삽입 로직 분기
            if (_icons.Count == 0)
            {
                // Case A: 0개 -> 1개 [A]
                _icons.Add(newIcon);
            }
            else if (_icons.Count == 1)
            {
                // Case B: 1개 -> 2개 [A] => [A, B, A'] (무한 루프 구조 생성)
                // 원래 있던 1개를 '더미'로 쓸 것이므로 복제본을 하나 더 만듦
                var firstSkillType = _icons[0].SkillType;
                var dummyIcon = CreateIconInstance(firstSkillType);

                _icons.Add(newIcon);   // B 추가
                _icons.Add(dummyIcon); // A' (더미) 추가
            }
            else
            {
                // Case C: 이미 더미가 있는 상태 [A, B, A'] -> [A, B, C, A']
                // 맨 뒤(더미) 바로 앞에 삽입해야 함
                int insertIndex = _icons.Count - 1;

                // 논리적 리스트 삽입
                _icons.Insert(insertIndex, newIcon);

                // 시각적(Hierarchy) 순서 맞춤 (더미 앞으로 이동)
                newIcon.IconPrefab.transform.SetSiblingIndex(insertIndex);
            }

            // 3. 위치 재정렬 (X 좌표 갱신)
            ArrangeIconsHorizontally();

            BlackboxHandle.Of(this).Write($"Skill Added. Total Count: {_icons.Count}");
        }

        // 아이콘 생성 및 초기화 헬퍼 메서드
        private IconInfo CreateIconInstance(SkillType skill)
        {
            if (!_configMap.TryGetValue(skill, out var prefab) || !prefab)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"[SkillManager] Prefab not found for {skill}"));

            var instance = Instantiate(prefab, _iconGroup, false);
            instance.SetActive(true);

            return new IconInfo { SkillType = skill, IconPrefab = instance };
        }

        // 현재 위치에서 가장 자연스러운 목표 인덱스를 찾는 로직
        private int FindSmartTargetIndex(SkillType targetSkill)
        {
            // 원본 데이터 구간(0 ~ N-1)에서 찾기
            int basicIndex = _icons.FindIndex(x => x.SkillType == targetSkill);
            if (basicIndex == -1) return -1;

            int lastRealIndex = _icons.Count - 2; // C (원본 마지막)
            int dummyIndex = _icons.Count - 1;    // A' (가짜 마지막)

            // [상황] 현재 'C'를 보고 있는데, 목표가 'A'다.
            // -> 앞으로 돌아가지 말고, 바로 옆에 있는 'A'(더미)로 가야 함.
            if (_targetIndex == lastRealIndex && basicIndex == 0)
            {
                return dummyIndex;
            }

            return basicIndex;
        }

        // 더미(A')에서 진짜(A)로 좌표 리셋
        private void ForceResetToStartIndex()
        {
            _targetIndex = 0;
            _targetPosX = 0; // 0번 위치 (X = 0)
            
            // 애니메이션 없이 즉시 좌표 변경
            _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);
            
            BlackboxHandle.Of(this).Write("Teleported logic applied (Infinite Scroll)");
        }
    }
}
