using System;
using Infrastructure;
using UnityEngine;

namespace UI
{
    public interface IView
    {
        event Action Destroyed;

        void SetParent(RectTransform parent);
        void Destroy();
    }

    public interface IEnablableView : IEnablable
    {
        event Action Enabling;
        event Action Disabling;
    }

    internal interface IInputEnabledView : IView
    {
        bool EnableInput { get; set; }
    }
}
