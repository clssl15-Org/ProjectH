namespace Sound
{
    public enum SfxName
    {
        None,
        Click,
        Hover,
    }

    public class SfxPlayManager : AudioPlayManager<SfxName>
    {
        private void Start()
        {
            SetVolume(GameContext.SfxVolume);
            GameContext.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            GameContext.SfxChanged -= SetVolume;
        }
    }
}
