using System;

namespace Infrastructure
{
    public interface IInputHub
    {
        void Add(IInputLayerSubject subject);
        void AddAfter(IInputLayerSubject target, IInputLayerSubject subject);
        void Remove(IInputLayerSubject subject);

        void Block(object requester);
        void Unblock(object requester);
    }

    public interface IInputLayerController
    {
        void Initialize(IInputHub inputHub);
    }

    public interface IInputLayerSubject
    {
        bool AllowInput { get; set; }
        bool IsTrigger { get; }

        event Action Destroying;
    }
    public interface IAwakableInputLayerSubject : IInputLayerSubject
    {
        event Action<bool> InputAwakeStateChanged;
    }
}
