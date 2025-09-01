using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterPlayerDetector : MonoBehaviour
{
    // Front
    public event Action<GameObject> PlayerDetected;
    public GameObject CurrentPlayer { get; private set; }

    // Internal
    private const string TargetTag = "Player";


    // Content
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(TargetTag))
            return;

        CurrentPlayer = collision.gameObject;
        PlayerDetected?.Invoke(CurrentPlayer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(TargetTag))
            return;

        CurrentPlayer = null;
    }
}
