using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters
{
    internal interface IMonsterInternal
    {
        int HP { get; set; }
        Direction Direction { get; set; }
        bool IsAlive { get; set; }

        int CurrentPlatform { get; set; }
        bool IgnorePlayerInteraction { get; set; }

        MonsterStats StatsInfo { get; }
        GameAssetLibrary SceneAssetsLibrary { get; }
        SpriteRenderer SpriteRenderer { get; }
        Rigidbody2D Rigidbody { get; }
        Collider2D Collider { get; }

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

#pragma warning disable IDE1006
        string name { get; }
        Transform transform { get; }
#pragma warning restore
        string FormatLogMessage(string message);
    }

    internal static class MonsterExtensions
    {
        public static bool IsValid(this IMonsterInternal monster)
            => monster as MonoBehaviour ?? false;
    }
}
