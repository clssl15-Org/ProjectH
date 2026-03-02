namespace Sound
{
    public enum SfxName
    {
        None,
        Click,
        Hover,
        Esc,
    }

    public class SfxPlayManager : AudioPlayManager<SfxName>
    {
        private void Start()
        {
            SetVolume(GameServices.SfxVolume);
            GameServices.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            GameServices.SfxChanged -= SetVolume;
        }
    }
}
