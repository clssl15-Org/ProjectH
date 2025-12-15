namespace Infrastructure.StateMachines.BT
{
    public partial class BTNode<TOwner, TBlackboard>
    {
        private partial class Hierarchy
        {
            private class Sequence : IHierarchyComponent
            {
                // Internal
                private readonly Hierarchy parent;


                // Content
                public Sequence(Hierarchy parent) => this.parent = parent;

                public bool ReadUpper(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    parent.GetPolicy(child, out _, out var lowerPriority);

                    if (lowerPriority && child.CheckCondition())
                    {
                        parent.CurrentChild?.Halt(DetailedNodeStatus.Preempted);
                        parent.CurrentChild = child;

                        return false;
                    }

                    return true;
                }

                public bool ReadCurrent(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    if (child.IsRunning)
                    {
                        parent.GetPolicy(child, out var self, out _);

                        if (self && !child.CheckCondition())
                            child.Halt(DetailedNodeStatus.AbortedSelf);
                        else
                            return false;
                    }

                    var result = parent.CurrentChild.NodeStatus;
                    parent.CurrentChild = null;

                    if (result == NodeStatus.Failure || result == NodeStatus.Aborted)
                    {
                        if (parent.LoopType == LoopType.Forced)
                            return false;

                        parent.OwnerNode.Halt(DetailedNodeStatus.ChildFailed);
                        return false;
                    }

                    if (parent.LoopType == LoopType.None && parent.Children[^1] == child)
                    {
                        parent.OwnerNode.Halt(DetailedNodeStatus.ChildCompleted);
                        return false;
                    }

                    return true;
                }

                public bool ReadLower(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    if (child.CheckCondition())
                    {
                        parent.CurrentChild = child;
                        return false;
                    }

                    if (parent.LoopType != LoopType.Forced)
                        parent.OwnerNode.Halt(DetailedNodeStatus.ChildFailed);

                    return false;
                }
            }
        }
    }
}
