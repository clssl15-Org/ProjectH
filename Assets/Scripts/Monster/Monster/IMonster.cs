using MonsterActions;
using UnityEngine;
using static MonsterActions.MonsterAction;

internal interface IMonster
{
    int HP { get; set; }
    Direction Direction { get; set; }
    bool IsAlive { get; set; }

    MonsterStats StatsInfo { get; }
    SceneAssetsLibrary SceneAssetsLibrary { get; }
    SpriteRenderer SpriteRenderer { get; }
    Rigidbody2D Rigidbody { get; }
    Collider2D Collider { get; }
    int BelongingPlatform { get; set; }
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
    void Die(bool succeed);
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
    public static bool IsValid(this IMonster monster)
        => monster as MonoBehaviour;
}
