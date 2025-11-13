using UnityEngine;
using Infrastructure;

namespace Actors
{
    public interface IMonster : IEventSubject<IMonster, IMonsterConditionData> 
    {
        int HP { get; }
        Direction Direction { get; }
        bool IsAlive { get; }

        int BelongingPlatform { get; }

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
