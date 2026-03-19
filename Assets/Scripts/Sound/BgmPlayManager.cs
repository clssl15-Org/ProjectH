using Infrastructure;

namespace Sound
{
    public enum BgmName
    {
        None,
        Title,
        Stage0,
        Stage1,
        Stage2,
        Stage2_Boss,
        Stage3,
        Stage3_Boss,
        Final_Boss,
    }

    public class BgmPlayManager : AudioPlayManager<BgmName>,
        IInjectable<GameServices>
    {
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices) =>
            _gameServices = gameServices;


        private void Start()
        {
            SetVolume(_gameServices.BgmVolume);
            _gameServices.BgmChanged += SetVolume;
        }

        private void OnDestroy()
        {
            _gameServices.BgmChanged -= SetVolume;
        }
    }
}
