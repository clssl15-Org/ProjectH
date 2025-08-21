using System;

namespace UniEngine.StateMachines.BT
{
    [Flags]
    public enum AbortPolicies
    {
        None = 0,
        Self = 1 << 0,
        LowerPriority = 1 << 1,

        Both = LowerPriority | Self
    }
}
