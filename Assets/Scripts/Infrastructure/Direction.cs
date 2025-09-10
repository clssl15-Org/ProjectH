using UnityEngine;

public enum Direction
{
    Left, 
    Center,
    Right
}

public static class DirectionTools
{
    public static Vector2 ToVector2(this Direction direction) => direction switch
    {
        Direction.Left => Vector2.left,
        Direction.Right => Vector2.right,
        _ => Vector2.zero
    };
}
