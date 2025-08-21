public class MonsterBlackboard
{
    // Front
    public bool IsDamaged { get; private set; }
    public int TakenDamage
    {
        get => _takenDamage;
        set
        {
            IsDamaged = true;
            _takenDamage = value;
        }
    }

    public bool Committing { get; set; } = false;
    public bool WasEngaged { get; set; } = false;


    // Internal
    int _takenDamage = 0;


    // Content
    public void ClearDamage()
    {
        IsDamaged = false;
        _takenDamage = 0;
    }
}
