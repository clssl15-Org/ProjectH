namespace Infrastructure
{
    public static class EnableSystemExtensions
    {
        public static EnableWithAnimation InitializeWithIEnablable(
            this EnableWithAnimation enabler,
            IEnablable component)
        {
            return enabler
                .SetAction(EnableEventType.Enabling, () =>
                {
                    component.gameObject.SetActive(true);
                    component.Enabling?.Invoke();
                })
                .SetAction(EnableEventType.Enabled, () => component.Enabled?.Invoke())
               .SetAction(EnableEventType.Disabling, () => component.Disabling?.Invoke())
               .SetAction(EnableEventType.Disabled, () =>
               {
                   component.gameObject.SetActive(false);
                   component.Disabled?.Invoke();
               });
        }
    }
}
