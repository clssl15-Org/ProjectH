using System;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
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
        [SerializeField] private float _iconWidth = 100f;   // ������ ���� (�̵� ����)
        [SerializeField] private float _moveSpeed = 10f;    // �̵� �ӵ� (Lerp Speed)

        [Serializable]
        private struct IconInfo
        {
            public SkillType SkillType;
            public GameObject IconPrefab;
        }
        [SerializeField] private IconInfo[] _iconConfigs;

        // [����] Inspector ������(IconInfo)�� ��Ÿ�� ������(RuntimeIcon)�� �и��Ͽ� ����
        private class RuntimeIcon
        {
            public SkillType SkillType;
            public RectTransform RectTransform;
        }

        private readonly List<RuntimeIcon> _icons = new();
        private Dictionary<SkillType, GameObject> _configMap;

        private int _targetIndex = 0;       // ��ǥ �ε���
        private float _targetPosX = 0f;     // ��ǥ X ��ǥ
        private bool _needsTeleport = false; // �̵� �Ϸ� �� �����̵� �ʿ� ����

        private SfxAudioController _sfxAudioController;

        private void Awake()
        {
            _sfxAudioController = GetComponent<SfxAudioController>();

            // O(1) �˻��� ���� ĳ��
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

            // 1. ���� ����
            for (int i = _iconGroup.childCount - 1; i >= 0; i--)
                Destroy(_iconGroup.GetChild(i).gameObject);
            _icons.Clear();

            if (skills.Length == 0) return;

            // [����] LINQ ���� �� ������ ���� ���� ����� List ���� �� ���� �߰�
            List<SkillType> skillsToCreate = skills.ToList();
            if (skillsToCreate.Count > 1)
            {
                skillsToCreate.Add(skillsToCreate[0]);
            }

            foreach (var skill in skillsToCreate)
            {
                if (!_configMap.TryGetValue(skill, out var prefab) || prefab == null)
                {
                    // [�ٽ�] Inspector�� �������� ��ϵ��� �ʾ� ������ ���õǴ� ���׸� ��� ���� ���� �α�
                    Debug.LogError($"[SkillSelectionManager] {skill} �������� _iconConfigs�� ����Ǿ����ϴ�! UI�� ǥ�õ��� �ʽ��ϴ�.");
                    continue;
                }

                var instance = Instantiate(prefab, _iconGroup, false);
                instance.SetActive(true);

                _icons.Add(new RuntimeIcon { SkillType = skill, RectTransform = instance.GetComponent<RectTransform>() });
            }

            // 3. �����ܵ��� ���η� �� ��ġ
            ArrangeIconsHorizontally();

            // �ʱ� ���� ����
            _targetIndex = 0;
            _targetPosX = 0;
            _iconGroup.anchoredPosition = Vector2.zero;
        }

        private void ArrangeIconsHorizontally()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                // [����] GetComponent ������� ���� (RuntimeIcon ���� �� �̸� ĳ����)
                _icons[i].RectTransform.anchoredPosition = new Vector2(i * _iconWidth, 0);
            }
        }

        public void OnSkillChanged(SkillType targetSkill)
        {

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
        }

        public void AddSkill(SkillType targetSkill)
        {

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

                // [����] ��Ÿ�ӿ� ��ų �߰� ��, ���� Ÿ���� ���� �ʿ� �־��ٸ� Ÿ�� �ε����� �о��־�� ��ġ�� Ƣ�� ����
                if (_targetIndex >= insertIndex)
                {
                    _targetIndex++;
                    _targetPosX = -1 * (_targetIndex * _iconWidth);
                    _iconGroup.anchoredPosition = new Vector2(_targetPosX, _iconGroup.anchoredPosition.y);
                }
            }

            ArrangeIconsHorizontally();
        }

        private RuntimeIcon CreateRuntimeIconInstance(SkillType skill)
        {
            if (!_configMap.TryGetValue(skill, out var prefab) || prefab == null)
            {
                throw new InvalidOperationException($"[SkillManager] Prefab not found for {skill}. Inspector�� Ȯ���ϼ���.");
            }

            var instance = Instantiate(prefab, _iconGroup, false);
            instance.SetActive(true);

            return new RuntimeIcon { SkillType = skill, RectTransform = instance.GetComponent<RectTransform>() };
        }

        private int FindSmartTargetIndex(SkillType targetSkill)
        {
            // [����] ���������� ƨ��� ���� ���� ���� '���� �ε��� ����'���� ���� Ž���Ͽ� ������ ���� ����
            int forwardIndex = _icons.FindIndex(_targetIndex, x => x.SkillType == targetSkill);

            // ���� ��ġ ���Ŀ� ���ٸ� ó������ �ٽ� Ž��
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

        }
    }
}
