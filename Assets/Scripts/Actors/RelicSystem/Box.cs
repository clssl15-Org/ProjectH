using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Sprite openedSprite;
    public GameObject Fsprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        Fsprite.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OpenBox();
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
    private void OpenBox()
    {
        spriteRenderer.sprite = openedSprite;
        StartCoroutine(GenerateUI());
    }
    IEnumerator GenerateUI()
    {
        yield return new WaitForSeconds(1f);
        // UI »ý¼º
        RelicManager.Instance.GetRandomRelicData();
    }
}
