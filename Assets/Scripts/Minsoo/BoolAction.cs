using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoolAction
{
    public bool value;

    // Returns true if the value transitioned from false to true
    public bool Started { get; private set; }

    // Returns true if the value transitioned from true to false
    public bool Canceled { get; private set; }

    // The elapsed time since this action was set to true.
    public float ActiveTime { get; private set; }

    // The elapsed time since this action was set to false.
    public float InactiveTime { get; private set; }

    bool previousValue;

    public void Initialize()
    {
        value = false;
        previousValue = true;
    }

    public void Reset()
    {
        Started = false;
        Canceled = false;
    }

    public void Update(float dt)
    {
        Started |= !previousValue && value;
        Canceled |= previousValue && !value;

        if (value)
        {
            ActiveTime += dt;
            InactiveTime = 0f;
        }
        else
        {
            ActiveTime = 0f;
            InactiveTime += dt;
        }

        previousValue = value;
    }
}
