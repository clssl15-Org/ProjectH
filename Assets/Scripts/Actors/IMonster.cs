using UnityEngine;
using Infrastructure;
using System;

namespace Actors
{
    public interface IMonster
    {
        int HP { get; }
        Direction Direction { get; }
        bool IsAlive { get; }

        event Action<IMonsterConditionData> ConditionChanged;
        event Action Destroyed;

        int MaxHP { get; }
        int BelongingPlatform { get; }

        void Destroy();

#pragma warning disable IDE1006
        string name { get; }
        Transform transform { get; }
#pragma warning restore
        string FormatLogMessage(string message);
    }

    public static class MonsterExtensions
    {
        public static bool IsValid(this IMonster monster)
            => monster as MonoBehaviour ?? false;
    }
}
