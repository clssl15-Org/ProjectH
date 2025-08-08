using System;
using UnityEngine;

public class MonsterHitted : MonoBehaviour
{
    public event Action<int> OnTakeDamage;

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage), $"damage 값은 0 이상이어야 합니다. 입력된 값: {damage}");
        }

        OnTakeDamage?.Invoke(damage);
    }
}
