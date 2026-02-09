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
        [field: SerializeField] public Player Player { get; set; }
        [field: SerializeField] public SkillType TargetSkillType { get; set; }


        private void Awake()
        {
            if (!Player)
                Player = GetComponent<Player>();
        }

        private void AddSkill()
        {
            if (!Player)
            {
                Debug.LogWarning("Player가 null입니다.", this);
                return;
            }

            if (!Player.TryGetComponent<SkillManager>(out var skillManager))
            {
                Debug.LogWarning("Player에서 SkillManager를 찾지 못하였습니다.", this);
                return;
            }


            var targetSkill = Player.GetComponentsInChildren<CharacterState>()
                .FirstOrDefault(skill => skill.SkillType == TargetSkillType);
            if (targetSkill == null)
            {
                Debug.LogWarning($"Player에서 Skill '{TargetSkillType}'을(를) 찾지 못하였습니다.", this);
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
                    if (GUILayout.Button("Add Skill"))
                        ((TestSkillAdder)target).AddSkill();
                }
                else
                    GUILayout.Label("Enter play mode to add skill", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
