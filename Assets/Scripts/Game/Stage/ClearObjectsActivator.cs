using UnityEngine;

namespace Game.Stage
{
    internal static class ClearObjectsActivator
    {
        public static void ActivateRootAndChildren(GameObject root)
        {
            if (!root)
                return;

            if (!root.activeSelf)
                root.SetActive(true);

            foreach (Transform child in root.transform)
                child.gameObject.SetActive(true);
        }
    }
}
