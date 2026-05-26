using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public GameObject Fsprite;
    public float value;

    private bool isPlayerInRange = false;

    private void Start()
    {
        Fsprite.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Apply();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = true;
        Fsprite.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = false;
        Fsprite.SetActive(false);
    }

    private void Apply()
    {
        RelicManager.Instance.player.PlayerHealth.HealByPercent(value * 0.01f);
        LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.PotionUse);
        Destroy(gameObject);
    }
}
