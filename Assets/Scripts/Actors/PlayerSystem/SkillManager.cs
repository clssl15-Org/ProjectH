using System;
using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public int SelectedSkillIndex => selectedIndex;
    public List<CharacterState> skills;
    public bool canChangeSkill = true;

    public CharacterState ultimateSkill;

    public CharacterActions characterActions;
    public CharacterStateController CharacterStateController { get; private set; }

    public event Action<string> OnSkillChanged;

    private DamageRoulette damageRoulette;
    private int selectedIndex = 0;

    private void Awake()
    {
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
        OnSkillChanged?.Invoke(skills[selectedIndex].name); // 선택 스킬이 변경되었다고 알림과 동시에 이름을 전달
        Debug.Log($"{skills[selectedIndex]} is Selected");
    }

    public void UseSkill()
    {
        if (skills.Count <= 0)
        {
            return;
        }

        CharacterStateController.EnqueueTransition(skills[selectedIndex]);
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
        skills.Add(skill);
    }
    public void AddUltimateSkill(CharacterState skill)
    {
        ultimateSkill = skill;
    }
}
