[System.Serializable]
public struct CharacterActions
{
    public BoolAction attack;

    public Vector2Action movement;

    public void Reset()
    {
        attack.Reset();

        movement.Reset();
    }

    public void InitializeActions()
    {
        attack = new BoolAction();
        attack.Initialize();

        movement = new Vector2Action();
        movement.Reset();
    }

    public void SetValues(InputHandler inputHandler)
    {
        if (inputHandler == null)
            return;

        attack.value = inputHandler.GetBool("Attack");

        movement.value = inputHandler.GetVector2("Movement");
    }

    public void Update(float dt)
    {
        attack.Update(dt);
    }
}
