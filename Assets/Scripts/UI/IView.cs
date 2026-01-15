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
}
