using System;
using System.Collections;
using System.Text;
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

    // Inspector
    [SerializeField, TextArea(3, 10)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();

    // Internal
    private GameObject laser;
    private bool attacking = false;
    private bool damaging = false;
    private bool alive = true;

    private Brain brain;


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


    private void Update()
    {
        brain?.Update();

#if UNITY_EDITOR
        UpdateStateDisplay();
#endif
    }

    private void UpdateStateDisplay()
    {
        sb.Clear();
        sb.AppendLine($"HP: {HP}");
        sb.AppendLine($"Direction: {Direction.ToString()}");
        sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");
        sb.AppendLine("----------------");
        sb.AppendLine($"Alive: {alive}");
        sb.AppendLine($"Attacking: {attacking}");
        sb.AppendLine($"GettingDamage: {damaging}");

        if (brain is not null)
        {
            sb.AppendLine("----------------");
            sb.AppendLine($"State: {brain.GetFullState()}");
        }

        stateDisplay = sb.ToString();
    }

    protected override void OnDamaged(int damage) => brain.TakeDamage(damage);

    protected bool TryAttack(Action callback = null)
    {
        if (attacking) return false;
        attacking = true;

        laser = Instantiate(laserPrefab);
        laser.transform.SetParent(transform);
        laser.name = laserPrefab.name;
        laser.transform.localPosition = laserPosition;
        laser.transform.localScale = Vector3.one;

        var attackTime = laserTime;
        StartCoroutine(DoAttack());

        IEnumerator DoAttack()
        {
            while (attacking && attackTime > 0)
            {
                attackTime -= Time.deltaTime;
                yield return null;
            }

            Destroy(laser);
            laser = null;

            attacking = false;
            callback?.Invoke();
        }

        return true;
    }

    protected void StopAttack() => attacking = false;

    protected bool TryGetDamage(Action callback = null)
    {
        if (damaging) return false;
        damaging = true;

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

            damaging = false;
            callback?.Invoke();
        }

        return true;
    }

    protected bool TryDie(Action callback = null)
    {
        if (!alive) return false;
        alive = false;

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
