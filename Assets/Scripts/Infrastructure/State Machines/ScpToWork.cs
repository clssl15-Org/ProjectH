using Infrastructure.StateMachines.Fsm;
using Infrastructure.StateMachines.Scp;
using UnityEngine;

namespace Infrastructure.StateMachines
{
    public static class StateMachineExtensions
    {
        public static Work ToWork<TBlackboard>(Sequence<TBlackboard> sequence)
            where TBlackboard : class, new() =>
            new Work()
                .AddUpdatedAction(() => sequence.Update(Time.deltaTime))
                .SetExitedAction(() => sequence.Stop());
    }
}
