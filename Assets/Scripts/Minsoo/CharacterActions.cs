using System.Diagnostics;

[System.Serializable]
public struct CharacterActions
{
    public BoolAction attack;
    public BoolAction Jump;

    public Vector2Action movement;

    public void Reset()
    {
        attack.Reset();
        Jump.Reset();

        movement.Reset();
    }

    public void InitializeActions()
    {
        attack = new BoolAction();
        attack.Initialize();

        Jump = new BoolAction();
        Jump.Initialize();

        movement = new Vector2Action();
        movement.Reset();
    }

    public void SetValues(InputHandler inputHandler)
    {
        if (inputHandler == null)
            return;

        attack.value = inputHandler.GetBool("Attack");
        Jump.value = inputHandler.GetBool("Jump");
        
        movement.value = inputHandler.GetVector2("Movement");
    }

    public void Update(float dt)
    {
        attack.Update(dt);
        Jump.Update(dt);
    }
}
