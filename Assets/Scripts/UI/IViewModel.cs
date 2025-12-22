using System;
using UnityEngine;

namespace UI
{
    public interface IViewModel : IDisposable
    {
        event Action Disposed;
    }

    public interface IPositionedVM : IViewModel
    {
        Vector2 WorldPosition { get; }
        Vector2 WorldBottomPosition { get; }
    }

    public interface IHealthRateVM : IViewModel
    {
        HealthRateData HealthRate { get; }
        event Action<HealthRateData> HealthRateChanged;
    }
}
