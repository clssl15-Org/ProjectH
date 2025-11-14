using System;
using UnityEngine;

namespace UI
{
    public interface IViewModel : IDisposable
    {
        event Action Disposed;
    }

    public interface IPositionedViewModel : IViewModel
    {
        Vector2 WorldPosition { get; }
    }
}
