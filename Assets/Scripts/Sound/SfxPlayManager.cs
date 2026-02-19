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
            SetVolume(gameServices.SfxVolume);
            gameServices.SfxChanged += SetVolume;
        }

        private void OnDestroy()
        {
            gameServices.SfxChanged -= SetVolume;
        }
    }
}
