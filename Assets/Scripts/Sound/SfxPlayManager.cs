using BlackThunder.BlackboxSystem;
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

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            using var _ = BlackboxHandle.Of(this).Scope("게임 서비스를 주입받습니다.").With(gameServices);

            _gameServices = gameServices;
        }


        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).Scope("SFX 재생 매니저 시작 설정을 적용합니다.");

            SetVolume(_gameServices.SfxVolume);
            _gameServices.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).Scope("SFX 재생 매니저를 정리합니다.");

            _gameServices.SfxChanged -= SetVolume;
        }
    }
}
