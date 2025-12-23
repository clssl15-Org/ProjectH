using UnityEngine;

namespace Infrastructure
{
    public interface IInjectable<T> where T : MonoBehaviour
    {
        void Inject(T item);
    }
}
