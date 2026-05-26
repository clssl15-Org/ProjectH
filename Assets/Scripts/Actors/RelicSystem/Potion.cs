using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public GameObject Fsprite;
    public float value;

    [SerializeField]
    private Vector3 fSpriteWorldOffset = new Vector3(0f, -0.94f, 0f);

    private void Start()
    {
        HideFSprite();
    }
    private void LateUpdate()
    {
        if (Fsprite != null && Fsprite.activeSelf)
        {
            UpdateFSpriteTransform();
        }
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

        ShowFSprite();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        HideFSprite();
    }

    private void ShowFSprite()
    {
        if (Fsprite == null)
        {
            return;
        }

        UpdateFSpriteTransform();
        Fsprite.SetActive(true);
    }

    private void HideFSprite()
    {
        if (Fsprite == null)
        {
            return;
        }

        Fsprite.SetActive(false);
    }

    private void UpdateFSpriteTransform()
    {
        Transform fSpriteTransform = Fsprite.transform;
        fSpriteTransform.position = transform.position + fSpriteWorldOffset;
        fSpriteTransform.rotation = Quaternion.identity;
    }

    private void Apply()
    {
        RelicManager.Instance.player.PlayerHealth.HealByPercent(value * 0.01f);
        LevelManager.Instance.SoundManager.PlayActionSound(PlayerAction.PotionUse);
        Destroy(gameObject);
    }
}
