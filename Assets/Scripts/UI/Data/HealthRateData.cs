using System;

namespace UI
{
    public interface IHealthRateVM : IPositionedViewModel
    {
        HealthRateData HealthRate { get; }
        event Action<HealthRateData> HealthRateChanged;
    }

    public record HealthRateData(float HealthRate)
    {
        public HealthRateData(float health, float maxHealth)
            : this((float)health / maxHealth) { }
    }
}
