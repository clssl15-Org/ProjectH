using System;
using UnityEngine;

namespace Infrastructure
{
    public interface IEnablable
    {
        Action OnEnabling { get; }
        Action OnEnabled { get; }
        Action OnDisabling { get;}
        Action OnDisabled { get; }

        void Enable();
        void Disable();
        void SetToEnabled();
        void SetToDisabled();

#pragma warning disable IDE1006
        GameObject gameObject { get; }
#pragma warning restore
    }
}
