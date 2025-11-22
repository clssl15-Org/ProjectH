using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public interface IClip
    {
        void Start(object blackborad, IEnumerable<ClipToken> befores);
        void Update(float deltaTime);
        public bool TryGetToken(out ClipToken token);
    }
}
