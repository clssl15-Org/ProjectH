using System;
using UnityEngine;

public class MonsterHitted : MonoBehaviour
{
    public event Action<DamageInfo> Damaged;

    public void TakeDamage(int damage) =>
        Damaged?.Invoke(new(damage));

    public void TakeDamage(int damage, Direction direction, float? knockbackForce = null) =>
        Damaged?.Invoke(new(damage, direction, knockbackForce));
}
