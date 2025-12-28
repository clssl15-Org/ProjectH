using UnityEngine;

public abstract class Relic : MonoBehaviour
{
    [SerializeField] protected RelicDataSO data;
    public RelicDataSO Data => data;

    public bool isReinforced;
    protected float value;

    public abstract void OnAcquire();
    public abstract void OnLose();
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