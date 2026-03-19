using System;
using Actors.PlayerSystem;
using BlackboxSystem;
using UnityEngine;

namespace UI.PlayerView
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkillSelectionManager), typeof(SkillCooltimeManager))]
    internal class SkillUI : MonoBehaviour
    {
        private SkillSelectionManager _skillSelectionManager;
        private SkillCooltimeManager _skillCooltimeManager;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _skillSelectionManager = GetComponent<SkillSelectionManager>();
            _skillCooltimeManager = GetComponent<SkillCooltimeManager>();

            BlackboxHandle.Of(this).Exert(_skillSelectionManager, "Awake from SkillUI");
            BlackboxHandle.Of(this).Exert(_skillCooltimeManager, "Awake from SkillUI");
        }

        // Skill Selecton Manager
        public void InitializeSkills(SkillType[] skills) => _skillSelectionManager.Initialize(skills);
        public void OnSkillChanged(SkillType targetSkill) => _skillSelectionManager.OnSkillChanged(targetSkill);
        public void AddSkill(SkillType targetSkill) => _skillSelectionManager.AddSkill(targetSkill);

        // Skill Cooltime Manager
        public void EnableCooltime(Func<float> getCooltimeRate) => _skillCooltimeManager.Enable(getCooltimeRate);
        public void DisableCooltime() => _skillCooltimeManager.Disable();
    }
}
