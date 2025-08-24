using System;
using UniEngine.StateMachines.FSM;

public abstract partial class Monster
{
    public partial class MonsterActionController : Work
    {
        public Monster Owner { get; protected set; }
        public float CallbackToleranceTime { get; set; } = 0f;

        public MonsterActionController(Monster monster)
        {
            Owner = monster;

            AddChild(new MonsteActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Alert.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Walk.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Run.ToString()));
        }

        public MonsterAction GetCurrentAction()
        {
            if (TryGetCurrentChild<MonsteActionState>(out var child))
            {
                if (child.Name == MonsterAction.Idle.ToString())
                    return MonsterAction.Idle;
                if (child.Name == MonsterAction.Alert.ToString())
                    return MonsterAction.Alert;
                if (child.Name == MonsterAction.Walk.ToString())
                    return MonsterAction.Walk;
                if (child.Name == MonsterAction.Run.ToString())
                    return MonsterAction.Run;
            }

            return MonsterAction.None;
        }
        public void DoAction(MonsterAction monsterAction, Action callback = null, float? playTime = null)
        {
            SetNextWith(monsterAction.ToString(), callback, playTime);
        }
    }
}
