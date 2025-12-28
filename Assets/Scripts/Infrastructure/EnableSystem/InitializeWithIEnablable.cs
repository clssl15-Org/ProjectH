namespace Infrastructure
{
    public static class EnableSystemExtensions
    {
        public static EnableWithAnimation InitializeWithIEnablable(
            this EnableWithAnimation enabler,
            IEnablable component,
            bool setGameObjectActive = true)
        {
            return enabler
                .SetAction(EnableEventType.Enabling, () =>
                {
                    if (setGameObjectActive)
                        component.gameObject.SetActive(true);

                    component.Enabling?.Invoke();
                })
                .SetAction(EnableEventType.Enabled, () => component.Enabled?.Invoke())
                .SetAction(EnableEventType.Disabling, () => component.Disabling?.Invoke())
                .SetAction(EnableEventType.Disabled, () =>
                {
                    if (setGameObjectActive)
                        component.gameObject.SetActive(false);

                    component.Disabled?.Invoke();
                });
        }
    }
}
