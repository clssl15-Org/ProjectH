using UnityEngine;

namespace Infrastructure
{
    public abstract class GameContext : MonoBehaviour
    {
        public abstract void Quit(object context = null);
    }
}
