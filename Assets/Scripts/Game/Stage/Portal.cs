using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public event Action Opening;
    public event Action Closing;
    public event Action<Action> MoveToNextLevel;

    [SerializeField]
    private GameObject Fsprite;
    [SerializeField]
    private GameObject mask;
    [SerializeField]
    private bool stageChange;

    private bool isPlayerInRange = false;
    private void Start()
    {
        Fsprite.SetActive(false);
    }
    private void OnEnable()
    {
        Opening?.Invoke();
        StartCoroutine(MoveMaskUp());
    }
    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Closing?.Invoke();
            MoveNextLevel();
        }
    }
    private void MoveNextLevel()
    {
        if (MoveToNextLevel != null)
            MoveToNextLevel(() => LevelManager.Instance.MoveNextLevel(stageChange));
        else
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
    IEnumerator MoveMaskUp()
    {
        Vector3 startPosition = mask.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 3.5f, 0);
        float elapsedTime = 0f;

        while (elapsedTime < 2)
        {
            mask.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / 2);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mask.transform.position = endPosition;
    }
}
