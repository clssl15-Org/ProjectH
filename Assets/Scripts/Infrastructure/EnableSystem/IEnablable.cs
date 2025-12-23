using System;
using UnityEngine;

namespace Infrastructure
{
    public interface IEnablable
    {
        Action Enabling { get; }
        Action Enabled { get; }
        Action Disabling { get;}
        Action Disabled { get; }

        void Enable();
        void Disable();
        void SetToEnabled();
        void SetToDisabled();

#pragma warning disable IDE1006
        GameObject gameObject { get; }
#pragma warning restore
    }
}
