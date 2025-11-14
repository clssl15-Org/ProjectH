using UnityEngine;
using Infrastructure;
using System;

namespace Actor
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


    public interface IPlayer
    {
        int HP { get; }
        bool IsAlive { get; }

        event Action<PlayerCondition> ConditionChanged;
        event Action Destroyed;

        int MaxHP { get; }

        int CurrentPlatform { get; }

#pragma warning disable IDE1006
        string name { get; }
        Transform transform { get; }
#pragma warning restore

    }

    class Foo
    {
        public void Update(PlayerCondition cond)
        {
            if (cond.IsAttack())
            {
                // ╬Нец ui ╤Г©Л╠Б
            }


        }
    }

}