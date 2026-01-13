using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public class StandaloneBehaviourManager : MonoBehaviour
    {
        [field: SerializeField] public bool StandaloneInitialize { get; set; } = true;
        [field: SerializeField] public bool StandaloneUpdate { get; set; } = true;

        private readonly List<IStandaloneUpdatable> _updatables = new();

        private void Start()
        {
            Scan(StandaloneInitialize);
        }

        public void Scan(bool initialize = false)
        {
            _updatables.Clear();

            var behaviours = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var behaviour in behaviours)
            {
                if (!behaviour)
                    continue;

                if (initialize && behaviour is IStandaloneInitializable initializable)
                    initializable.StandaloneInitialize();

                if (behaviour is IStandaloneUpdatable updatable)
                    _updatables.Add(updatable);
            }
        }

        private void Update()
        {
            if (!StandaloneUpdate)
                return;

            for (int i = _updatables.Count - 1; i >= 0; i--)
            {
                var updatable = _updatables[i];
                if (updatable == null || updatable.Equals(null))
                {
                    _updatables.RemoveAt(i);
                    continue;
                }

                updatable.StandaloneUpdate();
            }
        }
    }
}
