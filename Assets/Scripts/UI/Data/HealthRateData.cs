using UnityEngine;

namespace UI
{
    public readonly struct HealthRateData
    {
        public float HealthRate { get; }
        public float Health { get; }
        public float MaxHealth { get; }

        public HealthRateData(float healthRate)
        {
            HealthRate = NormalizeHealthRate(healthRate);
            Health = HealthRate;
            MaxHealth = 1f;
        }

        public HealthRateData(float health, float maxHealth)
        {
            if (maxHealth <= 0f)
            {
                Debug.LogWarning(
                    $"최대 체력은 0보다 커야 하지만 '{maxHealth}'이(가) 입력되었습니다. " +
                    $"체력 비율 계산을 위해 최대 체력을 '1'로 정규화합니다.");

                maxHealth = 1f;
            }

            Health = health;
            MaxHealth = maxHealth;
            HealthRate = NormalizeHealthRate(health / maxHealth);
        }

        private static float NormalizeHealthRate(float healthRate)
        {
            if (healthRate < 0f || healthRate > 1f)
            {
                var normalized = Mathf.Clamp01(healthRate);
                Debug.LogWarning(
                    $"체력 비율은 0 이상 1 이하의 값이어야 하지만 '{healthRate}'이(가) 입력되었습니다. " +
                    $"입력값을 '{normalized}'(으)로 정규화합니다.");

                healthRate = normalized;
            }

            return healthRate;
        }
    }
}
