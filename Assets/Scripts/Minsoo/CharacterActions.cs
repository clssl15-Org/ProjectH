using System.Diagnostics;

[System.Serializable]
public struct CharacterActions
{
    public BoolAction attack;
    public BoolAction jump;
    public BoolAction dash;
    public BoolAction eskill;
    public BoolAction rangedAttack;

    public Vector2Action movement;

    public void Reset()
    {
        attack.Reset();
        jump.Reset();
        dash.Reset();
        eskill.Reset();
        rangedAttack.Reset();

        movement.Reset();
    }

    public void InitializeActions()
    {
        attack = new BoolAction();
        attack.Initialize();

        jump = new BoolAction();
        jump.Initialize();

        dash = new BoolAction();
        dash.Initialize();

        eskill = new BoolAction();
        eskill.Initialize();

        rangedAttack = new BoolAction();
        rangedAttack.Initialize();

        movement = new Vector2Action();
        movement.Reset();
    }

    public void SetValues(InputHandler inputHandler)
    {
        if (inputHandler == null)
            return;

        attack.value = inputHandler.GetBool("Attack");
        jump.value = inputHandler.GetBool("Jump");
        dash.value = inputHandler.GetBool("Dash");
        eskill.value = inputHandler.GetBool("Eskill");
        rangedAttack.value = inputHandler.GetBool("RangedAttack");

        movement.value = inputHandler.GetVector2("Movement");
    }

    public void Update(float dt)
    {
        attack.Update(dt);
        jump.Update(dt);
        dash.Update(dt);
        eskill.Update(dt);
        rangedAttack.Update(dt);
    }
}
