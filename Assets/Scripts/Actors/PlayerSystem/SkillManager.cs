using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public int SelectedSkillIndex => selectedIndex;
    public List<CharacterState> skills = new List<CharacterState>();
    public bool canChangeSkill = true;

    public CharacterState ultimateSkill;

    public CharacterActions characterActions;
    public CharacterStateController CharacterStateController { get; private set; }

    public IEnumerable<SkillType> HavingSkills => skills.Select(skill => skill.SkillType);

    public SkillType SelectedSkillType => skills.Count > 0
        ? skills[selectedIndex].SkillType
        : SkillType.None;

    public event Action<SkillType> SkillAdded;
    public event Action<SkillType> SkillChanged;

    private static List<Type> _oldStats;

    /// <summary>
    /// 씬 전환 시 복원되는 스킬 목록(정적). 사망 후 런 리셋 시 비워야 합니다.
    /// </summary>
    public static void ClearPersistedSkillLoadout() => _oldStats = null;

    private DamageRoulette damageRoulette;
    private int selectedIndex = 0;

    private void Awake()
    {
        if (_oldStats != null)
            foreach (var oldSkillType in _oldStats)
            {
                var responding = gameObject.GetComponentInChildren(oldSkillType);
                if (responding)
                {
                    print("추가");
                    AddSkill(responding as CharacterState);
                }
            }

        CharacterStateController = this.transform.root.GetComponentInChildren<CharacterStateController>();
        characterActions = this.transform.root.GetComponentInChildren<CharacterBrain>().CharacterActions;
        damageRoulette = this.transform.root.GetComponentInChildren<DamageRoulette>();

        Init();
    }

    public void ChangeSkill()
    {
        if (skills.Count <= 0)
        {
            return;
        }

        if (!canChangeSkill)
        {
            return;
        }

        selectedIndex = (selectedIndex + 1) % skills.Count;
        SkillChanged?.Invoke(skills[selectedIndex].SkillType); // 선택 스킬이 변경되었다고 알림과 동시에 이름을 전달
        Debug.Log($"{skills[selectedIndex]} is Selected");
    }

    public void UseSkill()
    {
        if (skills.Count <= 0 || !damageRoulette.canUseSkill)
        {
            return;
        }

        if (IsAnySkillOnCooldown())
        {
            return;
        }

        CharacterStateController.EnqueueTransition(skills[selectedIndex]);
    }

    private bool IsAnySkillOnCooldown()
    {
        return GetComponentsInChildren<CooldownTimer>()
            .Any(timer => timer.CooldownType == CooldownType.Skill && timer.IsOnCooldown);
    }

    public void UseUltimate()
    {
        if (ultimateSkill == null)
        {
            return;
        }

        CharacterStateController.EnqueueTransition(ultimateSkill);
    }

    public void Init()
    {
        selectedIndex = 0;
    }
    public void AddSkill(CharacterState skill)
    {
        skill.enabled = true;
        skills.Add(skill);

        SkillAdded?.Invoke(skill.SkillType);
    }
    public void AddUltimateSkill(CharacterState skill)
    {
        skill.enabled = true;
        ultimateSkill = skill;

        SkillAdded?.Invoke(skill.SkillType);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.PlayerHasDied)
        {
            _oldStats = null;
            return;
        }

        _oldStats = skills.Select(s => s.GetType()).ToList();
    }
}
