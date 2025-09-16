namespace UniEngine.StateMachines.BT
{
    public partial class BTNode<TOwner, TBlackboard>
    {
        private partial class Hierarchy
        {
            private class Selector : IHierarchyComponent
            {
                // Internal
                private readonly Hierarchy hirearchy;


                // Content
                public Selector(Hierarchy hirearchy) => this.hirearchy = hirearchy;

                public bool ReadUpper(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    hirearchy.GetPolicy(child, out _, out var lowerPriority);

                    if (lowerPriority && child.CheckCondition())
                    {
                        hirearchy.CurrentChild?.Halt(DetailedNodeStatus.Preempted);
                        hirearchy.CurrentChild = child;

                        return false;
                    }

                    return true;
                }

                public bool ReadCurrent(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    if (child.IsRunning)
                    {
                        hirearchy.GetPolicy(child, out var self, out _);

                        if (self && !child.CheckCondition())
                            child.Halt(DetailedNodeStatus.AbortedSelf);
                        else
                            return false;
                    }

                    hirearchy.CurrentChild = null;

                    if (child.DetailedNodeStatus.IsSuccess())
                    {
                        hirearchy.OwnerNode.Halt(DetailedNodeStatus.ChildCompleted);
                        return false;
                    }

                    return true;
                }

                public bool ReadLower(IBTNodeInternal<TOwner, TBlackboard> child)
                {
                    if (child.CheckCondition())
                    {
                        hirearchy.CurrentChild = child;
                        return false;
                    }

                    if (hirearchy.Children[^1] == child)
                    {
                        if (hirearchy.LoopType == LoopType.None
                            || hirearchy.LoopType == LoopType.Conditional)
                        {
                            hirearchy.OwnerNode.Halt(DetailedNodeStatus.ChildFailed);
                        }

                        return false;
                    }

                    return true;
                }
            }
        }
    }
}
