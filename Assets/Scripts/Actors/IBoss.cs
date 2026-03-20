using Actors.Monsters;

namespace Actors
{
    public interface IBoss : IMonster
    {
        MonsterAudioPlayer AudioPlayer { get; } // HACK
        void Commence();
    }

    public interface IPlayerInitializable
    {
        void InitializePlayer(IPlayer player);
    }
}
 