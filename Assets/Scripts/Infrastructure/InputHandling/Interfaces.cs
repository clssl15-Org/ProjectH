using System;

namespace Infrastructure
{
    public interface IInputHub
    {
        void BlockAll();
        void BlockExcept(params IInputControllable[] controllables);
        void UnblockAll();
    }

    public interface IInputController
    {
        void Initialize(IInputHub inputHub);
    }

    public interface IInputControllable
    {
        bool AllowInput { get; set; }
        event Action Destroyed;
    }
}
