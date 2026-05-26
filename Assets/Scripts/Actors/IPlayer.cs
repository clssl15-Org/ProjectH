using System;
using System.Collections.Generic;
using Actors.PlayerSystem;
using Infrastructure;
using UnityEngine;
using World;

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
        MaxHealthChanged,
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


    public interface IPlayer : IInjectable<PlatformManager>, IInputLayerSubject
    {
        // ---------- Properties ----------
        int HP { get; }
        bool IsAlive { get; }
        bool Invincible { get; }

        event Action<PlayerCondition> ConditionChanged;

        int MaxHP { get; }
        int BaseMaxHP { get; }
        int CurrentPlatform { get; }
        Direction Direction { get; }

        // Skill
        int SelectedSkillIndex { get; }
        SkillType SelectedSkillType { get; }
        IEnumerable<SkillType> HavingSkills { get; }

        event Action<SkillType> SkillAdded;
        event Action<SkillType> SkillChanged;
        event Action<float> SkillRouletteApplied;
        event Action SkillRouletteCleared;

        float CurrentSkillCooldown { get; }
        float CurrentUltimateCooldown { get; }


        // ---------- Methods ----------
        // Attack
        void DefaultAttack();
        void RangedAttack();

        // Skill
        void UseSkill();
        void UseUltimate();
        void ChangeSkill();
        bool TrySkillRoulette(out DamageRoulette.Context context);
        void SetInvincibleOverride(object source, bool enabled);


        // ---------- MonoBehaviour ----------
#pragma warning disable IDE1006
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
#pragma warning restore
    }
}
