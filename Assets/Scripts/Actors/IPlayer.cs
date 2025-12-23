using System;
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


    public interface IPlayer : IInjectable<PlatformManager>
    {
        int HP { get; }
        bool IsAlive { get; }

        event Action<PlayerCondition> ConditionChanged;
        event Action Destroyed;

        int MaxHP { get; }
        int CurrentPlatform { get; }

<<<<<<< Updated upstream
=======
        void ChangeSkill();
        void UseSkill();
        void UseUltimate();
>>>>>>> Stashed changes
        void DefaultAttack();

        int SelectedSkillIndex { get; }
        float UltimateGauge { get; }

#pragma warning disable IDE1006
        string name { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
#pragma warning restore

    }
}
