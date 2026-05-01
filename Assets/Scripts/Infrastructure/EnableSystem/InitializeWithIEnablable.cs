using UnityEngine;

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
                    try
                    {
                        if (component == null || !component.gameObject)
                            return;
                    }
                    catch (MissingReferenceException)
                    {
                        // Ignore the exception if the component or its GameObject has been destroyed
                        return;
                    }

                    if (setGameObjectActive)
                        component.gameObject.SetActive(false);

                    component.OnDisabled?.Invoke();
                });
        }
    }
}
