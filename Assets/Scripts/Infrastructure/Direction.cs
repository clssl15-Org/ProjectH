using UnityEngine;

public enum Direction
{
    Center,
    Left, 
    Right,
}

public static class DirectionTools
{
    public static Direction ToDirection(this Vector3 vector) => ToDirection((Vector2)vector);
    public static Direction ToDirection(this Vector2 vector) => vector.x switch
    {
        var i when Mathf.Approximately(i, 0) => Direction.Center,
        > 0 => Direction.Right,
        < 0 => Direction.Left,
        _ => throw new System.ArgumentException($"유효하지 않은 벡터({vector})가 입력되었습니다.", nameof(vector))
    };

    public static Vector2 ToVector2(this Direction direction) => direction switch
    {
        Direction.Left => Vector2.left,
        Direction.Right => Vector2.right,
        _ => Vector2.zero
    };
    public static Vector3 ToVector3(this Direction direction) => direction switch
    {
        Direction.Left => Vector3.left,
        Direction.Right => Vector3.right,
        _ => Vector3.zero
    };

    public static Direction Flip(this Direction direction) => direction switch
    {
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => Direction.Center
    };
}
