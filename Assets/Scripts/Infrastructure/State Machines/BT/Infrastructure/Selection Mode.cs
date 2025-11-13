namespace Infrastructure.StateMachines.BT
{
    public enum EntryPolicy
    {
        CheckAlways,
        CheckOnOpen,
        Unconditional
    }

    public enum RerunPolicy
    {
        IgnoreIfRunning,
        Restart,
        EnsureRunningAndInjectInputs
    }

    public enum SelectionResult
    {
        NotMatched,
        ConditionFailure,
        Selected
    }
}
