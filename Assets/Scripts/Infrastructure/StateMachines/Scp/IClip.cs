using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public interface IClip
    {
        bool IsPlaying { get; }

        void Start(object blackboard, IEnumerable<ClipToken> befores);
        void Update(float deltaTime);
        bool TryGetToken(out ClipToken token);
        void Stop(object payload = null);
    }
}
