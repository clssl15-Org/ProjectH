using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(PlatformDetector))]
public class TestPlayer : MonoBehaviour
{
    // Front
    public int CurrentPlatform { get; private set; }

    // Property
    [SerializeField, Min(0)] private int attackPower;
    [SerializeField] private PlatformManager platformManager;

    // Inspector
    [SerializeField, TextArea(3, 10)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();

    // Internal
    private PlatformDetector platformDetector;


    // Content
    private void Awake()
    {
        if (!platformManager)
        {
            throw new InvalidOperationException(
                "TestPlayer 객체를 사용하려면 platformManager가 할당되어 있어야 합니다.");
        }

        platformDetector = GetComponent<PlatformDetector>();
        platformDetector.SetPlatformManager(platformManager);
    }

    private void Update()
    {
        if (platformDetector.TryGetCurrentPlatformId(out var platformId))
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

        if (detector)
            detector.TakeDamage(attackPower);
    }


    private void UpdateStateDisplay()
    {
        sb.Clear();
        sb.AppendLine($"Current Platform: {(CurrentPlatform >= 0 ? CurrentPlatform : "null")}");

        stateDisplay = sb.ToString();
    }
}
