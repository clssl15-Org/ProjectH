namespace Actors.Monsters.Stage3Bosses
{
    internal interface ITwinBoss : IMonsterInternal
    { 
        const string IsAwake = nameof(IsAwake);
        bool IsExhausted { get; set; }

        void InitializePlayer(IPlayer player);
        void DoAwake();
        void Revive(float hpRate);
        void Die();
    }
}

