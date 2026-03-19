using System;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors
{
    public interface IMonster :
        IInjectable<GameAssetLibrary>,
        IInjectable<Configuration>,
        IInjectable<PlatformManager>
    {
        int HP { get; }
        Direction Direction { get; }
        bool IsAlive { get; }

        event Action<MonsterConditionData> ConditionChanged;
        event Action Destroyed;

        int MaxHP { get; }
        int CurrentPlatform { get; }
        bool IgnorePlayerInteraction { get; }

        void Initialize(
            GameAssetLibrary gameAssetLibrary,
            Configuration configuration,
            PlatformManager platformManager);

        void Destroy();

#pragma warning disable IDE1006
        string name { get; }
        GameObject gameObject { get; }
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
