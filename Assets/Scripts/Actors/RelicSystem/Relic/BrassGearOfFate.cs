using Actors.PlayerSystem;
using UnityEngine;

public class BrassGearOfFate : Relic, IRelicPlayerRebindHandler
{
    private const int HighMultiplierStartIndex = 3;

    public override void OnAcquire()
    {
        SyncProbabilities();
    }

    public void RebindPlayer(Player player)
    {
        SyncProbabilities(player);
    }

    protected override void OnLoseCore()
    {
        SyncProbabilities(excludeFromSum: this);
    }

    private void SyncProbabilities(Player? targetPlayer = null, BrassGearOfFate? excludeFromSum = null)
    {
        var player = targetPlayer != null
            ? targetPlayer
            : RelicManager.Instance?.player ?? FindObjectOfType<Player>();

        var roulette = GetRoulette(player);
        roulette.Probabilities = BuildProbabilities(GetCombinedValue(excludeFromSum));
    }

    private float GetCombinedValue(BrassGearOfFate? excludeFromSum = null)
    {
        if (RelicManager.Instance == null)
            return 0f;

        float sum = RelicManager.Instance.GetValueSum(data.RelicNumber);

        if (excludeFromSum != null)
            sum -= excludeFromSum.value;

        return Mathf.Max(0f, sum);
    }

    private static int[] BuildProbabilities(float combinedValue)
    {
        int[] probabilities = DamageRoulette.DefaultProbabilities;
        int probabilityShift = Mathf.RoundToInt(combinedValue);

        for (int i = 0; i < probabilities.Length; i++)
        {
            if (i < HighMultiplierStartIndex)
                probabilities[i] -= probabilityShift;
            else
                probabilities[i] += probabilityShift;
        }

        return probabilities;
    }

    private static DamageRoulette GetRoulette(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        var roulette = player.GetComponent<DamageRoulette>();
        if (roulette == null)
            throw new System.InvalidOperationException("[BrassGearOfFate] 플레이어의 DamageRoulette 컴포넌트를 찾을 수 없습니다.");

        return roulette;
    }
}
