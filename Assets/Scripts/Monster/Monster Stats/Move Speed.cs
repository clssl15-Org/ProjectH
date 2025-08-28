using System;

public enum MoveSpeed
{
    None,
    NotMove,
    Slow,
    Normal,
    Fast
}

public static class MoveSpeedExtensions
{
    public static float ToFloat(this MoveSpeed moveSpeed) => moveSpeed switch
    {
        MoveSpeed.None => 0f,
        MoveSpeed.NotMove => 0f,
        MoveSpeed.Slow => 0.7f,
        MoveSpeed.Normal => 1f,
        MoveSpeed.Fast => 1.5f,

        _ => throw new ArgumentException(
            $"정의되지 않은 MoveSpeed 타입({moveSpeed})을 환산할 수 없습니다.",
            nameof(moveSpeed))
    };
}
