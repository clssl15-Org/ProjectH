using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public static class WorldInteractionInputBlocker
    {
        private static readonly HashSet<object> Requesters = new();

        public static bool IsBlocked => Requesters.Count > 0;

        public static void Block(object requester)
        {
            if (requester == null)
                return;

            Requesters.Add(requester);
        }

        public static void Unblock(object requester)
        {
            if (requester == null)
                return;

            Requesters.Remove(requester);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Requesters.Clear();
        }
    }
}
