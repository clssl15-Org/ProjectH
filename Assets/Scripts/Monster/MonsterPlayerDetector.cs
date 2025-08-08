using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterPlayerDetector : MonoBehaviour
{
    // Front
    public event Action<GameObject> OnPlayerDetected;

    // Internal
    private const string TargetTag = "Player";

    // Content
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(TargetTag))
            OnPlayerDetected?.Invoke(collision.gameObject);
    }
}
