using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Vector2Action
{
    public Vector2 value;

    public void Reset()
    {
        value = Vector2.zero;
    }

    public bool Detected
    {
        get
        {
            return value != Vector2.zero;
        }
    }

    public bool Right
    {
        get
        {
            return value.x > 0;
        }
    }

    public bool Left
    {
        get
        {
            return value.x < 0;
        }
    }

    public bool Up
    {
        get
        {
            return value.y > 0;
        }
    }

    public bool Down
    {
        get
        {
            return value.y < 0;
        }
    }
}