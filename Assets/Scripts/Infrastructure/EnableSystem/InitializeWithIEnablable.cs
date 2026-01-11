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

                    component.OnEnabling?.Invoke();
                })
                .SetAction(EnableEventType.Enabled, () => component.OnEnabled?.Invoke())
                .SetAction(EnableEventType.Disabling, () => component.OnDisabling?.Invoke())
                .SetAction(EnableEventType.Disabled, () =>
                {
                    if (setGameObjectActive)
                        component.gameObject.SetActive(false);

                    component.OnDisabled?.Invoke();
                });
        }
    }
}
