using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [SerializeField]
    public CharacterState initialState = null;

    readonly Dictionary<string, CharacterState> states = new Dictionary<string, CharacterState>();

    private Queue<CharacterState> transitionQueue = new Queue<CharacterState>();

    public Vector2 InputMovementReference { get; private set; }

    public Vector2 MovementReferenceRight { get; private set; }

    private bool machineStarted = false;

    public CharacterBrain CharacterBrain { get; private set; }
    public CharacterActor CharacterActor { get; private set; }

    public CharacterState CurrentState { get; private set; }

    public CharacterState PreviousState { get; private set; }
    public SpriteRenderer PlayerSpriteRenderer { get; private set; }
    public Animator Animator => CharacterActor.Animator;

    public CharacterState GetState(string stateName)
    {
        CharacterState state = null;
        states.TryGetValue(stateName, out state);

        return state;
    }

    public CharacterState GetState<T>() where T : CharacterState
    {
        string stateName = typeof(T).Name;
        return GetState(stateName);
    }

    public void EnqueueTransition<T>() where T : CharacterState
    {
        CharacterState state = GetState<T>();

        if (state == null)
        {
            return;
        }

        transitionQueue.Enqueue(state);
    }

    public void EnqueueTransition(CharacterState state)
    {
        if (state == null)
        {
            return;
        }

        transitionQueue.Enqueue(state);
    }

    public void ForceState(CharacterState state)
    {
        if (state == null)
        {
            return;
        }

        PreviousState = CurrentState;
        CurrentState = state;

        PreviousState.ExitBehaviour(Time.deltaTime);

        //if(CurrentState.RuntimeAnimatorController != null)
        //{
        //    Animator.runtimeAnimatorController = CurrentState.RuntimeAnimatorController;
        //}

        CurrentState.EnterBehaviour(Time.deltaTime);
    }

    public void ForceState<T>() where T : CharacterState
    {
        CharacterState state = GetState<T>();

        if (state == null)
        {
            return;
        }

        ForceState(state);
    }

    private void AddStates()
    {
        CharacterState[] statesArray = GetComponents<CharacterState>();
        for (int i = 0; i < statesArray.Length; i++)
        {
            CharacterState state = statesArray[i];
            string stateName = state.GetType().Name;

            if (GetState(stateName) != null)
            {
                continue;
            }

            states.Add(stateName, state);
        }
    }

    private bool CheckForTransitions()
    {
        CurrentState.CheckExitTransition();

        CharacterState nextState = null;

        while (transitionQueue.Count != 0)
        {
            CharacterState thisState = transitionQueue.Dequeue();
            if (thisState == null)
            {
                continue;
            }

            bool success = thisState.CheckEnterTransition(CurrentState);

            if (success)
            {
                nextState = thisState;

                PreviousState = CurrentState;
                CurrentState = nextState;

                return true;
            }
        }

        return false;
    }

    private void UpdateMovementData(Vector2 movementInput)
    {
        MovementReferenceRight = Vector2.right;
        InputMovementReference = MovementReferenceRight * movementInput.x;

        ChangeFlipX(InputMovementReference);
    }

    public void ChangeFlipX(Vector2 inputValue)
    {
        if (inputValue.x == 0f)
            return;

        if (inputValue.x > 0f)
            PlayerSpriteRenderer.flipX = false;
        else
            PlayerSpriteRenderer.flipX = true;
    }

    private void Awake()
    {
        CharacterBrain = this.transform.root.GetComponentInChildren<CharacterBrain>();
        CharacterActor = this.transform.root.GetComponentInChildren<CharacterActor>();
        PlayerSpriteRenderer = this.transform.root.GetComponentInChildren<SpriteRenderer>();

        AddStates();
    }

    private void FixedUpdate()
    {
        if (!machineStarted)
        {
            if (initialState == null)
            {
                return;
            }

            CurrentState = initialState;
            CurrentState.EnterBehaviour(0f);

            machineStarted = true;

            Animator.runtimeAnimatorController = CurrentState.RuntimeAnimatorController;
        }

        if (CharacterBrain != null)
            UpdateMovementData(CharacterBrain.CharacterActions.movement.value);

        bool valiidTransition = CheckForTransitions();
        
        transitionQueue.Clear();

        float dt = Time.deltaTime;
        if (valiidTransition)
        {
            PreviousState.ExitBehaviour(dt);

            Animator.runtimeAnimatorController = CurrentState.RuntimeAnimatorController;

            CurrentState.EnterBehaviour(dt);
        }

        CurrentState.PreUpdateBehaviour(dt);
        CurrentState.UpdateBehaviour(dt);
        CurrentState.PostUpdateBehaviour(dt);
    }
}