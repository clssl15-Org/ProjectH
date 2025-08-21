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
        public static NodeStatus ToNodeStatus(this DetailedNodeStatus s) => s switch
        {
            DetailedNodeStatus.NeverRun => NodeStatus.NeverRun,
            DetailedNodeStatus.Running => NodeStatus.Running,
            DetailedNodeStatus.Success or DetailedNodeStatus.ChildCompleted => NodeStatus.Success,
            DetailedNodeStatus.Failure or DetailedNodeStatus.ChildFailed => NodeStatus.Failure,
            _ => NodeStatus.Aborted
        };
    }
}
