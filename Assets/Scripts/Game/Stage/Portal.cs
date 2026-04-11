using System;
using System.Collections;
using System.Collections.Generic;
using Infrastructure;
using UI;
using UnityEngine;

public class Portal : MonoBehaviour, IInjectable<GameServices>  
{
    public event Action Opening;
    public event Action Closing;
    public event Action<Action> MoveToNextLevel;

    [SerializeField]
    private GameObject Fsprite;
    [SerializeField]
    private GameObject mask;
    [SerializeField]
    private LevelType levelType;

    [SerializeField]
    private bool toDesignatedLevel;
    [SerializeField]
    private string designatedLevelName;
    private GameServices gameServices;

    private bool isPlayerInRange = false;

    void IInjectable<GameServices>.Inject(GameServices gameServices) =>
        this.gameServices = gameServices;

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
        if (toDesignatedLevel)
        {
            FindAnyObjectByType<DarkscreenUI>(FindObjectsInactive.Include).CloseScreen(() =>
                gameServices.ChangeScene(designatedLevelName, this));
            return;
        }

        if (MoveToNextLevel != null)
            MoveToNextLevel(() => LevelManager.Instance.MoveNextLevel(levelType));
        else
            LevelManager.Instance.MoveNextLevel(levelType);
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
