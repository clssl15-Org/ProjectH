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

    public class BgmPlayManager : AudioPlayManager<BgmName>
    {
        private void Start()
        {
            SetVolume(GameServices.BgmVolume);
            GameServices.BgmChanged += SetVolume;
        }

        private void OnDestroy()
        {
            GameServices.BgmChanged -= SetVolume;
        }
    }
}
