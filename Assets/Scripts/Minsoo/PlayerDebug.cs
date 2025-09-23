using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;
    [SerializeField]
    int damageAmount = 10;

    private void Awake()
    {
        if (!playerHealth)
            playerHealth = this.transform.root.GetComponentInChildren<PlayerHealth>();
    }
    public void DamageToPlayer()
    {
        playerHealth.TakeDamage(damageAmount);
    }
}

[CustomEditor(typeof(PlayerDebug))]
public class DebugButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerDebug script = (PlayerDebug)target;
        if (GUILayout.Button("Damage to Player"))
        {
            script.DamageToPlayer();
        }
    }
}