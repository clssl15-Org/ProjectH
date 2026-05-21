using System;
using UnityEngine;

public abstract class Relic : MonoBehaviour
{
    [SerializeField] protected RelicDataSO data;
    public RelicDataSO Data => data;
    public float Value => value;

    public bool isReinforced;
    protected float value;

    public abstract void OnAcquire();

    /// <summary>획득 직전 <see cref="RelicManager"/>가 강화 여부와 적용 수치(상한 적용 후)를 설정합니다.</summary>
    public void PrepareAcquire(bool reinforced, float acquisitionValue)
    {
        isReinforced = reinforced;
        value = acquisitionValue;
    }

    public void OnLose()
    {
        OnLoseCore();
        RelicManager.Instance.RemoveRelic(data.RelicNumber);
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