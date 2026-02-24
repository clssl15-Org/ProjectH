using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Sprite openedSprite;
    public GameObject Fsprite;

    private SpriteRenderer spriteRenderer;
    private bool isPlayerInRange = false;
    private bool isBoxOpened = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        Fsprite.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isBoxOpened)
        {
            OpenBox();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBoxOpened)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = true;
        Fsprite.SetActive(true);

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isBoxOpened)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = false;
        Fsprite.SetActive(false);
    }
    private void OpenBox()
    {
        isBoxOpened = true;
        spriteRenderer.sprite = openedSprite;
        Fsprite.SetActive(false);
        StartCoroutine(GenerateUI());
    }
    IEnumerator GenerateUI()
    {
        yield return new WaitForSeconds(0.5f);
        // UI »ý¼º
        RelicManager.Instance.GetRandomRelicData();
    }
}
