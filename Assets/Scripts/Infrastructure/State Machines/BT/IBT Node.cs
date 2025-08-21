using System;

namespace UniEngine.StateMachines.BT
{
    public interface IBTNode : IDisposable
    {
        string Name { get; }
        IBTNode Parent { get; }

        object Owner { get; }
        object Blackboard { get; }

        AbortPolicies AbortPolicy { get; }
        LoopType LoopType { get; set; }

        bool IsRunning { get; }
        NodeStatus NodeStatus { get; }
        DetailedNodeStatus DetailedNodeStatus { get; }

        bool IsDisposed { get; }


        void Tick();
        void Halt();
    }

    internal interface IBTNodeInternal : IBTNode
    {
        new IBTNodeInternal Parent { get; set; }
        bool RetickNow { get; set; }

        bool CheckCondition();
        void Halt(DetailedNodeStatus reason);
    }
}
