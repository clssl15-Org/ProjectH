using Infrastructure;

namespace Sound
{
    public enum SfxName
    {
        None,
        Click,
        Hover,
        Esc,
        Text,
        Revive,
        CoinThrow,
        CoinDrop,
    }

    public class SfxPlayManager : AudioPlayManager<SfxName>,
        IInjectable<GameServices>
    {
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices) =>
            _gameServices = gameServices;


        private void Start()
        {
            SetVolume(_gameServices.SfxVolume);
            _gameServices.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            _gameServices.SfxChanged -= SetVolume;
        }
    }
}
