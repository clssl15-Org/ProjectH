namespace Actors
{
    public interface IBoss : IMonster
    {
        void Commence();
    }

    public interface IPlayerIInitializable
    {
        void InitializePlayer(IPlayer player);
    }
}
