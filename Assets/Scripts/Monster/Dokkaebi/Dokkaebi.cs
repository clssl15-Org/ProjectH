using System;
using System.Collections;
using UnityEngine;

public partial class Dokkaebi : Monster
{
    // Property
    [Header("Dokkaebi")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Vector3 laserPosition;
    [SerializeField, Min(0)] private float laserTime;
    [SerializeField, Min(0)] private float damagingTime;
    [SerializeField, Min(0)] private float dyingTime;

    // Internal
    private GameObject laser;
    private DokkaebiBrain brain;


    // Content
    protected override void Awake()
    {
        base.Awake();

        if (!laserPrefab)
        {
            throw new InvalidOperationException(
               $"laserPrefab이 등록되어 있지 않기 때문에 도깨비 '{name}'을(를) 시작할 수 없습니다.");
        }
    }

    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        brain = new(this);
        brain.Enter();
    }

    protected override void Update()
    {
        brain?.Update();
        base.Update();
    }

    protected override string GetDisplayContent()
    {
        if (brain == null)
            return base.GetDisplayContent();

        return
            base.GetDisplayContent() +
            "\n----------------\n" +
            brain.GetFullState();
    }

    protected override void OnDamaged(int damage)
    {
        brain.TakeDamage(damage);
        base.OnDamaged(damage);
    }

    protected override bool DoAttack(Action callback = null)
    {
        if (Attacking) return false;
        Attacking = true;

        laser = Instantiate(laserPrefab);
        laser.transform.SetParent(transform);
        laser.name = laserPrefab.name;
        laser.transform.localPosition = laserPosition;
        laser.transform.localScale = Vector3.one;

        var attackTime = laserTime;
        StartCoroutine(DoAttack());

        IEnumerator DoAttack()
        {
            while (Attacking && attackTime > 0)
            {
                attackTime -= Time.deltaTime;
                yield return null;
            }

            Destroy(laser);
            laser = null;

            Attacking = false;
            callback?.Invoke();
        }

        return true;
    }

    protected bool TryGetDamage(Action callback = null)
    {
        if (Damaging) return false;
        Damaging = true;

        var damageTime = damagingTime;
        StartCoroutine(DoGetDamage());

        IEnumerator DoGetDamage()
        {
            if (gameObject.TryGetComponent<SpriteRenderer>(out var sr))
                sr.color = new Color(1, 1, 1, 0.5f);

            while (damageTime > 0)
            {
                damageTime -= Time.deltaTime;
                yield return null;
            }

            if (gameObject.TryGetComponent(out sr))
                sr.color = Color.white;

            Damaging = false;
            callback?.Invoke();
        }

        return true;
    }

    protected bool TryDie(Action callback = null)
    {
        if (!Alive) return false;
        Alive = false;

        var dieTime = dyingTime;
        StartCoroutine(DoDie());

        IEnumerator DoDie()
        {
            if (gameObject.TryGetComponent<SpriteRenderer>(out var sr))
                sr.color = new Color(1, 1, 1, 0.5f);

            while (dieTime > 0)
            {
                dieTime -= Time.deltaTime;
                yield return null;
            }

            callback?.Invoke();
            Destroy(gameObject);
        }

        return true;
    }

    protected override void OnDestroy()
    {
        brain?.Dispose();
        base.OnDestroy();
    }
}
