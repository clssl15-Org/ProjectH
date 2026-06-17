using System;
using Actors.PlayerSystem;
using UnityEngine;

namespace UI.PlayerView
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkillRouletteManager), typeof(SkillSelectionManager), typeof(SkillCooltimeManager))]
    internal class SkillUI : MonoBehaviour
    {
        private SkillRouletteManager _skillRouletteManager;
        private SkillSelectionManager _skillSelectionManager;
        private SkillCooltimeManager _skillCooltimeManager;

        private void Awake()
        {

            _skillRouletteManager = GetComponent<SkillRouletteManager>();   
            _skillSelectionManager = GetComponent<SkillSelectionManager>();
            _skillCooltimeManager = GetComponent<SkillCooltimeManager>();

        }

        // Skill Roulette Manager
        public void EnableRoulette(int index, Action callback) => _skillRouletteManager.Enable(index, callback);
        public void DisableRoulette() => _skillRouletteManager.Disable();

        // Skill Selecton Manager
        public void InitializeSkills(SkillType[] skills) => _skillSelectionManager.Initialize(skills);
        public void OnSkillChanged(SkillType targetSkill) => _skillSelectionManager.OnSkillChanged(targetSkill);
        public void AddSkill(SkillType targetSkill) => _skillSelectionManager.AddSkill(targetSkill);

        public void ShowRouletteResult(float bonus) => _skillRouletteManager.ShowResult(bonus);
        public void ClearRouletteResult() => _skillRouletteManager.ClearResult();

        // Skill Cooltime Manager
        public void EnableCooltime(Func<float> getCooltimeRate) => _skillCooltimeManager.Enable(getCooltimeRate);
        public void DisableCooltime() => _skillCooltimeManager.Disable();
    }
}
