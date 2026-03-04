using System.Collections;
using System.Collections.Generic;
using Game.Stage;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private GameObject Fsprite;
    [SerializeField]
    private bool stageChange;

    private bool isPlayerInRange = false;
    private void Start()
    {
        Fsprite.SetActive(false);
    }
    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            MoveNextLevel();
        }
    }
    private void MoveNextLevel()
    {
        LevelManager.Instance.MoveNextLevel(stageChange);
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
}