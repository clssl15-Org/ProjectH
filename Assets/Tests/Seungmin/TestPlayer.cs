using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(PlatformDetector))]
public class TestPlayer : MonoBehaviour
{
    // Front
    public int CurrentPlatform { get; private set; }

    // Property
    [SerializeField] private PlatformManager _platformManager;

    // Inspector
    [Header("Attack")]
    [Min(0)] public int AttackPower;
    [Header("Knockback")]
    public bool UseKnockback = true;
    public bool UseDefaultKnockbackForce = true;
    public float KnockbackForce = 0;
    [Header("State Disply")]
    [SerializeField, TextArea(3, 10)]
    private string _stateDisplay = string.Empty;
    private readonly StringBuilder _sb = new();

    // Internal
    private PlatformDetector _platformDetector;


    // Content
    private void Awake()
    {
        if (!_platformManager)
            throw new InvalidOperationException(
                $"{typeof(TestPlayer).Name} 객체를 사용하려면 {nameof(_platformManager)} 컴포넌트가 할당되어 있어야 합니다.");

        _platformDetector = GetComponent<PlatformDetector>();
        _platformDetector.SetPlatformManager(_platformManager);
    }

    private void Update()
    {
        if (_platformDetector.TryGetCurrentPlatformId(out var platformId))
            CurrentPlatform = platformId;
        else
            CurrentPlatform = -1;

#if UNITY_EDITOR
        UpdateStateDisplay();
#endif
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var detector = collision.gameObject
            .GetComponentInChildren<MonsterHitted>();

        if (!detector)
            return;

        if (UseKnockback)
        {
            var dir = (detector.transform.position - transform.position).ToDirection();
            detector.TakeDamage(AttackPower, dir, UseDefaultKnockbackForce ? null : KnockbackForce);
        }
        else
        {
            detector.TakeDamage(AttackPower);
        }
    }


    private void UpdateStateDisplay()
    {
        _sb.Clear();
        _sb.AppendLine($"Current Platform: {(CurrentPlatform >= 0 ? CurrentPlatform : "null")}");

        _stateDisplay = _sb.ToString();
    }
}
