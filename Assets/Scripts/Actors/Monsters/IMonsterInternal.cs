using System;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    internal interface IMonsterInternal
    {
        int HP { get; set; }
        Direction Direction { get; set; }
        bool IsAlive { get; set; }

        event Action<IMonsterConditionData> ConditionChanged;
        event Action Destroyed;

        int CurrentPlatform { get; set; }
        bool IgnorePlayerInteraction { get; set; }

        MonsterStats StatsInfo { get; }
        GameAssetLibrary GameAssetsLibrary { get; }
        Configuration Configuration { get; }
        SpriteRenderer SpriteRenderer { get; }
        Rigidbody2D Rigidbody { get; }
        Collider2D Collider { get; }

        PlatformManager PlatformManager { get; }
        PlatformDetector PlatformDetector { get; }
        GameObject DetectedPlayer { get; }

        MonsterAnimationPlayer AnimationPlayer { get; }
        StandaloneHitAction StandaloneHitAction { get; }
        MonsterActionController ActionController { get; }

        #region Low-level Actions
        bool TryMove();
        bool TryMove(Direction direction);
        void StopMoving();
        
        void Knockback(Direction direction, float? knockbackForce = null);
        void NotifyCondition(IMonsterConditionData data);
        #endregion

        #region High-level Actions
        bool TryDoAction(
            MonsterActionPlayInfo playInfo,
            out ActionResult reason,
            bool stopPreviousAction = true,
            bool allowRestart = false);

        MonsterActionType GetCurrentAction();
        bool TryGetCurrentAction(out string name);

        void StopCurrentAction();
        #endregion

        void Destroy();

#pragma warning disable IDE1006
        string name { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
#pragma warning restore
        string FormatLogMessage(string message);
    }

    internal static class MonsterExtensions
    {
        public static bool IsValid(this IMonsterInternal monster)
            => monster as MonoBehaviour ?? false;
    }
}
