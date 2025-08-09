using System.Diagnostics;

[System.Serializable]
public struct CharacterActions
{
    public BoolAction attack;
    public BoolAction jump;

    public Vector2Action movement;

    public void Reset()
    {
        attack.Reset();
        jump.Reset();

        movement.Reset();
    }

    public void InitializeActions()
    {
        attack = new BoolAction();
        attack.Initialize();

        jump = new BoolAction();
        jump.Initialize();

        movement = new Vector2Action();
        movement.Reset();
    }

    public void SetValues(InputHandler inputHandler)
    {
        if (inputHandler == null)
            return;

        attack.value = inputHandler.GetBool("Attack");
        jump.value = inputHandler.GetBool("Jump");
        
        movement.value = inputHandler.GetVector2("Movement");
    }

    public void Update(float dt)
    {
        attack.Update(dt);
        jump.Update(dt);
    }
}
