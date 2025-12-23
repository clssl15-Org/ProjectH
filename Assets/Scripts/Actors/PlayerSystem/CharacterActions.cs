using System.Diagnostics;

namespace Actors.PlayerSystem
{
    [System.Serializable]
    public struct CharacterActions
    {
        public BoolAction attack;
        public BoolAction jump;
        public BoolAction dash;
        public BoolAction changeSkill;
        public BoolAction rangedAttack;
        public BoolAction ultimate;
        public BoolAction useSkill;

        public Vector2Action movement;

        public void Reset()
        {
            attack.Reset();
            jump.Reset();
            dash.Reset();
            changeSkill.Reset();
            rangedAttack.Reset();
            ultimate.Reset();
            useSkill.Reset();

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

            changeSkill = new BoolAction();
            changeSkill.Initialize();

            rangedAttack = new BoolAction();
            rangedAttack.Initialize();

            ultimate = new BoolAction();
            ultimate.Initialize();

            useSkill = new BoolAction();
            useSkill.Initialize();

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
            changeSkill.value = inputHandler.GetBool("ChangeSkill");
            rangedAttack.value = inputHandler.GetBool("RangedAttack");
            ultimate.value = inputHandler.GetBool("Ultimate");
            useSkill.value = inputHandler.GetBool("UseSkill");

            movement.value = inputHandler.GetVector2("Movement");
        }

        public void Update(float dt)
        {
            attack.Update(dt);
            jump.Update(dt);
            dash.Update(dt);
            changeSkill.Update(dt);
            rangedAttack.Update(dt);
            ultimate.Update(dt);
            useSkill.Update(dt);
        }
    }
}
