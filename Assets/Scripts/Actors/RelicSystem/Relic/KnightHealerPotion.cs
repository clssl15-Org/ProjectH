using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightHealerPotion : Relic
{
    [SerializeField]
    private GameObject potionPrefab;

    private float launchForce = 0.2f;

    public override void OnAcquire()
    {
        var manager = RelicManager.Instance;
        if (manager.IsAcquisitionFlowActive)
        {
            manager.AcquisitionUiClosed += SpawnPotionWhenUiClosed;
            return;
        }

        SpawnPotionAt(GetSpawnPosition(manager));
    }

    private void SpawnPotionWhenUiClosed()
    {
        var manager = RelicManager.Instance;
        manager.AcquisitionUiClosed -= SpawnPotionWhenUiClosed;
        SpawnPotionAt(GetSpawnPosition(manager));
    }

    private Vector3 GetSpawnPosition(RelicManager manager) =>
        manager.AcquisitionSourcePosition ?? transform.position;

    private void SpawnPotionAt(Vector3 position)
    {
        GameObject potion = Instantiate(potionPrefab, position, Quaternion.identity);
        Potion potionScript = potion.GetComponent<Potion>();
        potionScript.value = value;

        Rigidbody2D rb = potion.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        float randomX = Random.Range(-0.1f, 0.1f);
        float randomY = Random.Range(0.1f, 0.2f);
        Vector2 launchDirection = new Vector2(randomX, randomY).normalized;

        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

        float randomTorque = Random.Range(-1f, 1f);
        rb.AddTorque(randomTorque, ForceMode2D.Impulse);
    }
    protected override void OnLoseCore()
    {
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.AcquisitionUiClosed -= SpawnPotionWhenUiClosed;
        }
    }
}
