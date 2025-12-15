using System;

namespace Infrastructure.StateMachines.Fsm
{
    public interface IChildWork : IDisposable
    {
        string Name { get; init; }
        bool Active { get; set; }
        bool IsDisposed { get; }

        void Enter(params object[] args);
        void Update();
        void Exit();
    }
}
