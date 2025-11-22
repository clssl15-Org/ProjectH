using UnityEngine;

namespace Infrastructure
{
    public class DeactivateOnAwake : MonoBehaviour
    {
        [SerializeField] private bool _deactivateOnAwake = true;

        private void Awake()
        {
            if (_deactivateOnAwake)
                gameObject.SetActive(false);
        }
    }
}
