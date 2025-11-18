namespace Actors.Monsters.Stage3Bosses
{
    internal interface ITwinBoss : IMonsterInternal
    { 
        const string IsAwaken = "IsAwaken";

        void Initialize(IPlayer player);
        void DoAwake();
        void Die();
    }
}

