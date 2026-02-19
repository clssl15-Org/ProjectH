using System;
using System.Collections.Generic;
using Actors.PlayerSystem;
using Infrastructure;
using UnityEngine;
using World;
using static DamageRoulette;

namespace Actors
{
    public enum PlayerCondition
    {
        None,
        General,
        Heal,
        Attack,
        RangedAttack,
        ESkill,
        Ultimate,
        Damage,
        Die
    }

    public static class PlayerConditionExtensions
    {
        public static bool IsAttack(this PlayerCondition condition) =>
            condition == PlayerCondition.Attack
            || condition == PlayerCondition.RangedAttack
            || condition == PlayerCondition.ESkill
            || condition == PlayerCondition.Ultimate;
    }


    public interface IPlayer : IInjectable<PlatformManager>, IInputControllable
    {
        // ---------- Properties ----------
        int HP { get; }
        bool IsAlive { get; }

        IEnumerable<SkillType> HavingSkills { get; }
        SkillType SelectedSkillType { get; }

        event Action<PlayerCondition> ConditionChanged;
        event Action<SkillType> SkillAdded;
        event Action<SkillType> SkillChanged;

        int MaxHP { get; }
        int CurrentPlatform { get; }
        Direction Direction { get; }

        // Skill
        int SelectedSkillIndex { get; }


        // ---------- Methods ----------
        // Attack
        void DefaultAttack();
        void RangedAttack();

        // Skill
        void UseSkill();
        void UseUltimate();
        void ChangeSkill();
        bool TrySkillRoulette(out Context rouletteDTO);


        // ---------- MonoBehaviour ----------
#pragma warning disable IDE1006
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
#pragma warning restore
    }
}
