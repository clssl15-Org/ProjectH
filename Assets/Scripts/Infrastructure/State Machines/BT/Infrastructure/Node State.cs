namespace UniEngine.StateMachines.BT
{
    public enum DetailedNodeStatus
    {
        NeverRun,
        Running,
        Success,
        Failure,
        AbortedSelf,
        Preempted,
        ChildFailed,
        ChildCompleted,
        HaltedByParent,
        HaltedExternal,
        Disposed
    }
    
    public enum NodeStatus
    {
        NeverRun,
        Running,
        Success,
        Failure,
        Aborted
    }

    public static class Tools
    {
        public static NodeStatus ToNodeStatus(this DetailedNodeStatus status) => status switch
        {
            DetailedNodeStatus.NeverRun => NodeStatus.NeverRun,
            DetailedNodeStatus.Running => NodeStatus.Running,
            DetailedNodeStatus.Success or DetailedNodeStatus.ChildCompleted => NodeStatus.Success,
            DetailedNodeStatus.Failure or DetailedNodeStatus.ChildFailed => NodeStatus.Failure,
            _ => NodeStatus.Aborted
        };

        public static bool IsSuccess(this DetailedNodeStatus status) => IsSuccess(status.ToNodeStatus());
        public static bool IsSuccess(this NodeStatus status) => status == NodeStatus.Success;
    }
}
