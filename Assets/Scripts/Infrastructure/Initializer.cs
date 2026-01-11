using UnityEngine;

namespace Infrastructure
{
    public class Initializer : MonoBehaviour
    {
        [field: SerializeField] public bool InitializeOnStart { get; set; } = true;

        private void Start()
        {
            var behaviours = FindObjectsOfType<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (!behaviour)
                    continue;

                if (behaviour is IInitializable initializable)
                    initializable.Initialize();
            }
        }
    }
}
