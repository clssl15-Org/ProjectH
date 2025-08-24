namespace UniEngine.StateMachines.BT
{
    public partial class BTNode<TOwner, TBlackboard>
    {
        private partial class Hierarchy
        {
            private class Selector : IHierarchyComponent
            {
                // Internal
                private readonly Hierarchy parent;


                // Content
                public Selector(Hierarchy parent) => this.parent = parent;

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

                    var reason = child.NodeStatus;
                    parent.CurrentChild = null;

                    if (reason == NodeStatus.Success)
                    {
                        if (parent.LoopType == LoopType.None)
                            parent.OwnerNode.Halt(DetailedNodeStatus.ChildCompleted);

                        return false;
                    }

                    if (parent.Children[^1] == child)
                    {
                        if (parent.LoopType != LoopType.Forced)
                            parent.OwnerNode.Halt(DetailedNodeStatus.ChildFailed);

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

                    if (parent.Children[^1] == child)
                    {
                        if (parent.LoopType != LoopType.Forced)
                            parent.OwnerNode.Halt(DetailedNodeStatus.ChildFailed);

                        return false;
                    }

                    return true;
                }
            }
        }
    }
}
