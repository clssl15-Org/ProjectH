using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(MonsterHitted))]
public abstract class Monster : MonoBehaviour
{
    // Front
    public int HP { get; protected set; }

    // Property
    protected Rigidbody2D Rigidbody { get; private set; }
    protected int BelongingPlatform { get; set; } = -1;

    private MonsterHitted hitDetector;
    private MonsterPlayerDetector playerDetector;


    // Content
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

        hitDetector = GetComponent<MonsterHitted>();
        hitDetector.OnTakeDamage += OnDamaged;

        playerDetector = GetComponentInChildren<MonsterPlayerDetector>();

        if (playerDetector)
            playerDetector.OnPlayerDetected += OnPlayerDetected;
        else
            Debug.LogWarning(
                $"이 몬스터({name})은(는) {nameof(MonsterPlayerDetector)}를 가지고 있지 않습니다.\n" +
                "플레이어 감지 기능이 정상적으로 작동하지 않을 수 있습니다.");
    }

    protected abstract void OnPlayerDetected(GameObject player);
    protected abstract void OnDamaged(int damage);

    private void OnDestroy()
    {
        if (hitDetector)
            hitDetector.OnTakeDamage -= OnDamaged;

        if (playerDetector)
            playerDetector.OnPlayerDetected -= OnPlayerDetected;
    }
}
