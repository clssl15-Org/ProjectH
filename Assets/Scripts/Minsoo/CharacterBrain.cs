using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBrain : MonoBehaviour
{
    private InputHandler inputHandler;
    private CharacterActions characterActions = new CharacterActions();

    public CharacterActions CharacterActions => characterActions;

    private bool canPlayerControl = true;
    public bool CanPlayerControl => canPlayerControl;

    public void UpdateBrainValues(float dt)
    {
        if (Time.timeScale == 0)
            return;
        if(!canPlayerControl)
            return;

        characterActions.SetValues(inputHandler);
        characterActions.Update(dt);
        
    }

    private void Awake()
    {
        characterActions.InitializeActions();
        inputHandler = GetComponent<InputHandler>();
    }

    private void OnEnable()
    {
        characterActions.InitializeActions();
        characterActions.Reset();
    }

    private void OnDisable()
    {
        characterActions.InitializeActions();
        characterActions.Reset();
    }

    private void FixedUpdate()
    {
        float dt = Time.deltaTime;
        
        characterActions.Reset();
        UpdateBrainValues(dt);
    }
}
