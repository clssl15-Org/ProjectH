namespace Actors.Monsters.Stage3Bosses
{
    internal interface ITwinBoss : IMonsterInternal
    { 
        const string IsAwaken = "IsAwaken";
        bool IsExhausted { get; set; }

        void Initialize(IPlayer player);
        void DoAwake();
        void Revive(float hpRate);
        void Die();
    }
}

