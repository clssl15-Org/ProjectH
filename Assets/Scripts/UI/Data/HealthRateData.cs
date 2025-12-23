using UnityEngine;

namespace UI
{
    public readonly struct HealthRateData
    {
        public float HealthRate { get; }

        public HealthRateData(float healthRate)
        {
            if (healthRate < 0f || healthRate > 1f)
            {
                var normalized = Mathf.Clamp01(healthRate);
                Debug.LogWarning(
                    $"체력 비율은 0 이상 1 이하의 값이어야 하지만 '{healthRate}'이(가) 입력되었습니다. " +
                    $"입력값을 '{normalized}'(으)로 정규화합니다.");

                healthRate = normalized;
            }

            HealthRate = healthRate;
        }
        public HealthRateData(float health, float maxHealth) : this(health / maxHealth) { }
    }
}
