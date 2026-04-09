using System;
using UnityEngine;

public abstract class Relic : MonoBehaviour
{
    [SerializeField] protected RelicDataSO data;
    public RelicDataSO Data => data;
    public float Value => value;

    public bool isReinforced;
    protected float value;
    public event Action OnRelicLose;

    public abstract void OnAcquire();
    public void OnLose()
    {
        OnLoseCore();
        OnRelicLose?.Invoke();
    }
    protected abstract void OnLoseCore();
    public virtual void OnReinforcedAcquire()
    {
        if (isReinforced)
        {
            value = data.CoinFlipValue;
        }
        else
        {
            value = data.BaseValue;
        }
    }
}