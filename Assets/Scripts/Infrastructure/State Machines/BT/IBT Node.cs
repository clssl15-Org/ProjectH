using System;
using System.Collections.Generic;

namespace UniEngine.StateMachines.BT
{
    public interface IBTNode : IDisposable
    {
        string Name { get; }

        SelectionOptions SelectionOption { get; }
        LoopType LoopType { get; set; }

        bool IsRunning { get; }
        NodeStatus NodeStatus { get; }
        DetailedNodeStatus DetailedNodeStatus { get; }

        bool IsDisposed { get; }

        bool CheckCondition();
        /// <summary>
        /// Executes a single update tick.
        /// </summary>
        void Tick();
        /// <summary>
        /// Halts the node immediately.
        /// </summary>
        /// <remarks>
        /// Call this from outside the node to force a stop. To report that the node has
        /// finished its work from within the node, call Complete method(internal) instead.
        /// </remarks>
        void Halt();

        void SelectChild(IEnumerable<SelectionRequest> requests);
        SelectionResult CheckSelectionCondition(SelectionRequest request);

        object GetOwner();
        object GetBlackboard();
    }

    public interface IBTNode<out TOwner, out TBlackboard> : IBTNode where TOwner : class where TBlackboard : class, new()
    {
        IBTNode<TOwner, TBlackboard> ParentNode { get; }

        TOwner Owner { get; }
        TBlackboard Blackboard { get; }
    }


    internal interface IBTNodeInternal<TOwner, TBlackboard> : IBTNode<TOwner, TBlackboard> where TOwner : class where TBlackboard : class, new()
    {
        bool IsSelectable { get; }

        /// <summary>
        /// Indicates whether the current parent's child should be ticked again immediately.
        /// </summary>
        bool RetickNow { get; set; }
        IBTNodeInternal<TOwner, TBlackboard> CurrentChild { get; }

        void SetParent(IBTNodeInternal<TOwner, TBlackboard> parent);
        void Halt(DetailedNodeStatus reason);

        void SelectChildInternal(IEnumerable<SelectionRequest> requests);
    }
}
