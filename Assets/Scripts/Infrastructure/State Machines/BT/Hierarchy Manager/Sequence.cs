namespace UniEngine.StateMachines.BT
{
    public partial class BTNode
    {
        private partial class HierarchyManager
        {
            private class Sequence : IHierarchyComponent
            {
                // Internal
                private readonly HierarchyManager parent;


                // Content
                public Sequence(HierarchyManager parent) => this.parent = parent;

                public bool ReadUpper(IBTNodeInternal child)
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

                public bool ReadCurrent(IBTNodeInternal child)
                {
                    if (child.IsRunning)
                    {
                        parent.GetPolicy(child, out var self, out _);

                        if (self && !child.CheckCondition())
                            parent.CurrentChild.Halt(DetailedNodeStatus.AbortedSelf);
                        else
                            return false;
                    }

                    var reason = parent.CurrentChild.NodeStatus;
                    parent.CurrentChild = null;

                    if (reason == NodeStatus.Failure)
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

                public bool ReadLower(IBTNodeInternal child)
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
