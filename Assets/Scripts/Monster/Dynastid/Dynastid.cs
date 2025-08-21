using MonsterBT;

public class Dynastid : Monster
{
    // Internal
    private class DynastidBrain : MonsterBrain
    {
        public DynastidBrain(Dynastid owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new NotValidPlatform())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(true))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol()))));
            AddChild(new Dead());
        }
    }


    // Content
    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;
    
        Brain = new DynastidBrain(this);
    }

    protected override void OnDamaged(int damage)
    {
        Brain.Blackboard.TakenDamage += damage;
        base.OnDamaged(damage);
    }
}
