using System.Linq;
using Actors.PlayerSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tests.Seungmin
{
    public class TestSkillAdder : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private SkillType[] _skillsToAddOnStart;
        [Space]
        [SerializeField] private SkillType _skillToAdd;


        private void Awake()
        {
            if (!_player)
                _player = GetComponent<Player>();
        }

        private void Start()
        {
            foreach (var skillType in _skillsToAddOnStart)
                AddSkill(skillType);
        }

        private void AddSkill(SkillType targetSkillType)
        {
            if (!_player)
            {
                Debug.LogWarning("Player가 null입니다.", this);
                return;
            }

            if (!_player.TryGetComponent<SkillManager>(out var skillManager))
            {
                Debug.LogWarning("Player에서 SkillManager를 찾지 못하였습니다.", this);
                return;
            }

            var targetSkill = _player.GetComponentsInChildren<CharacterState>()
                .FirstOrDefault(skill => skill.SkillType == targetSkillType);
            if (targetSkill == null)
            {
                Debug.LogWarning($"Player에서 Skill '{targetSkillType}'을(를) 찾지 못하였습니다.", this);
                return;
            }

            skillManager.AddSkill(targetSkill);
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(TestSkillAdder))]
        private class TestSkillAdderEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space(8);

                if (Application.isPlaying)
                {
                    var target = (TestSkillAdder)base.target;

                    if (GUILayout.Button("Add Skill"))
                        target.AddSkill(target._skillToAdd);
                }
                else
                    GUILayout.Label("Enter play mode to add skill", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
