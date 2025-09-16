namespace UniEngine.StateMachines.BT
{
    /// <summary>
    /// Defines how a parent node should handle evaluation
    /// after all child nodes have been processed.
    /// </summary>
    public enum LoopType
    {
        /// <summary>
        /// Do not loop. Evaluation stops after the last child.
        /// </summary>
        None,

        /// <summary>
        /// Restart from the first child on success only (mode-dependent).
        /// </summary>
        /// <remarks>
        /// <para>Selector: restart when any child succeeds; all-fail stops.</para>
        /// <para>Sequence: restart only when the final child succeeds; mid-failure stops.</para>
        /// </remarks>
        Conditional,

        UntilSuccess,

        /// <summary>
        /// Always restart evaluation from the first child,
        /// regardless of the children's results.
        /// </summary>
        Forced
    }
}
