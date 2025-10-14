using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField]
    private int damage = 10;

    public static Action onRangedAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<IDamageable>(out var damageableObject))
            return;

        damageableObject.TakeDamage(damage);
        onRangedAttack?.Invoke();
    }
}
