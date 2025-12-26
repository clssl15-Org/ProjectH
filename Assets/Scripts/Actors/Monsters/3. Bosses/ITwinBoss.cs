namespace Actors.Monsters.Bosses
{
    internal interface ITwinBoss 
        : IBoss, IPlayerInitializable, IMonsterInternal
    { 
        const string IsAwake = nameof(IsAwake);
        bool IsExhausted { get; set; }

        void Revive(float hpRate);
        void SetToDead();
    }
}
