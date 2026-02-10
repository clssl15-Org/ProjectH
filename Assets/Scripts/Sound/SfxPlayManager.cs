namespace Sound
{
    public enum SfxType
    {
        None,
        Click,
        Hover,
    }

    public class SfxPlayManager : AudioPlayManager<SfxType>
    {
        private void Start()
        {
            SetVolume(GameContext.SfxVolume / 100f);
            GameContext.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            GameContext.SfxChanged -= SetVolume;
        }
    }
}
