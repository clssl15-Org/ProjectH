using System;
using System.Collections.Generic;
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

        event Action<PlayerCondition> ConditionChanged;

        int MaxHP { get; }
        int CurrentPlatform { get; }

        // Skill
        int SelectedSkillIndex { get; }


        // ---------- Methods ----------
        // Attack
        void DefaultAttack();
        void RangedAttack();

        // Skill
        void UseSkill();
        void UseUltimate();
        void ChangeSkill(int skillIndex);
        void ApplyRandomSkillBuff(float factor);


        // ---------- MonoBehaviour ----------
#pragma warning disable IDE1006
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
#pragma warning restore
    }
}
