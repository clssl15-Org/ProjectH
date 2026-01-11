namespace Actors
{
    public interface IBoss : IMonster
    {
        void Commence();
    }

    public interface IPlayerInitializable
    {
        void InitializePlayer(IPlayer player);
    }
}
 