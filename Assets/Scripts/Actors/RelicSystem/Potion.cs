using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public GameObject Fsprite;
    public float value;

    private void Start()
    {
        Fsprite.SetActive(false);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.F))
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

        Fsprite.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Fsprite.SetActive(false);
    }

    private void Apply()
    {
        RelicManager.Instance.player.GetComponent<PlayerHealth>().HealByPercent(value);
        Destroy(gameObject);
    }
}
