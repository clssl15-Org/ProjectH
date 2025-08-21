using System;
using UnityEditor.Experimental.GraphView;
using static UniEngine.StateMachines.Tools;

namespace UniEngine.StateMachines.BT
{
    public partial class BTNode : IBTNode, IBTNodeInternal
    {
        // Front
        public string Name { get; }

        public IBTNode Parent
        {
            get => ((IBTNodeInternal)this).Parent;

            private set => ((IBTNodeInternal)this).Parent = value == null ? null :
                value as IBTNodeInternal ?? throw new ArgumentException(Ctx($"Parent node must be of type '{typeof(IBTNodeInternal).Name}' or its derivative type."), nameof(value));
        }
        IBTNodeInternal IBTNodeInternal.Parent { get; set; }

        public object Owner => Parent?.Owner ?? _owner;
        public object Blackboard => Parent?.Blackboard ?? (_blackboard ??= GetBlackboard());

        public AbortPolicies AbortPolicy { get; set; } = AbortPolicies.None;

        /// <summary>
        /// Gets or sets how the children are evaluated:
        /// <see cref="HierarchyMode.Selector"/> or <see cref="HierarchyMode.Sequence"/>.
        /// </summary>
        /// <value>
        /// The default is <see cref="HierarchyMode.Selector"/>.
        /// </value>
        public HierarchyMode HierarchyMode
        {
            get => Hierarchy.HierarchyMode;
            set => Hierarchy.HierarchyMode = value;
        }
        public LoopType LoopType
        {
            get => Hierarchy.LoopType;
            set => Hierarchy.LoopType = value;
        }

        public bool IsRunning => DetailedNodeStatus == DetailedNodeStatus.Running;
        public NodeStatus NodeStatus => DetailedNodeStatus.ToNodeStatus();
        public DetailedNodeStatus DetailedNodeStatus { get; private set; } = DetailedNodeStatus.NeverRun;

        public bool IsDisposed { get; private set; } = false;

        // Property
        private HierarchyManager Hierarchy => _hierarchy ??= new(this);
        private HierarchyManager _hierarchy;

        /// <summary>
        /// Indicates whether the current parent's child should be ticked again immediately.
        /// </summary>
        protected bool RetickNow
        {
            get => ((IBTNodeInternal)this).RetickNow;
            set => ((IBTNodeInternal)this).RetickNow = value;
        }
        bool IBTNodeInternal.RetickNow { get; set; } = false;

        // Internal
        private readonly object _owner;
        private object _blackboard;
        private bool isDisposing = false;


        // Content
        public BTNode(object owner = null, string name = null)
        {
            _owner = owner;
            Name = name ?? GetName(GetType());
        }
        protected virtual object GetBlackboard() => null;


        public void Tick()
        {
            ThrowIfDisposed();

            try
            {
                if (!IsRunning)
                {
                    DetailedNodeStatus = DetailedNodeStatus.Running;
                    OnOpen();
                }

                if (!IsRunning)
                    return;

                OnTick();

                if (!IsRunning)
                    return;

                _hierarchy?.Tick();
            }
            catch
            {
                Halt(DetailedNodeStatus.Failure);
                throw;
            }
        }

        protected virtual bool CheckCondition()
        {
            ThrowIfDisposed();
            return true;
        }
        bool IBTNodeInternal.CheckCondition() => CheckCondition();

        protected virtual void OnOpen() { }
        protected virtual void OnTick() { }

        protected void Complete() => Complete(true);
        /// <summary>
        /// Marks the operation as complete.
        /// </summary>
        /// <param name="succeed">Indicates whether the operation completed successfully.</param>
        protected void Complete(bool succeed)
        {
            Halt(succeed ? DetailedNodeStatus.Success : DetailedNodeStatus.Failure);
        }

        /// <summary>
        /// Manually halts the node.<br/>
        /// To signal the completion of the activity internally, use the Complete method instead.
        /// </summary>
        public void Halt() => Halt(DetailedNodeStatus.HaltedExternal);
        void IBTNodeInternal.Halt(DetailedNodeStatus reason) => Halt(reason);
        private void Halt(DetailedNodeStatus reason)
        {
            if (IsDisposed) return;
            if (!IsRunning) return;

            _hierarchy?.Halt();
            OnHalt(reason);

            DetailedNodeStatus = reason;
        }
        protected virtual void OnHalt(DetailedNodeStatus reason) { }


        #region Child Management
        public BTNode AddChild(IBTNode child)
        {
            ThrowIfDisposed();
            Hierarchy.AddChild(child);

            return this;
        }

        public BTNode RemoveChild(object name)
        {
            ThrowIfDisposed();
            Hierarchy.RemoveChild(GetName(name));

            return this;
        }
        public BTNode RemoveChild(Predicate<IBTNode> predicate)
        {
            ThrowIfDisposed();
            Hierarchy.RemoveChild(predicate);

            return this;
        }
        #endregion


        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name, Ctx("This node has been disposed."));
        }

        public void Dispose()
        {
            if (isDisposing || IsDisposed) return;
            isDisposing = true;

            Halt(DetailedNodeStatus.Disposed);
            DetailedNodeStatus = DetailedNodeStatus.Disposed;

            OnDispose();
            _hierarchy?.Dispose();

            Parent = null;
            IsDisposed = true;
        }
        protected virtual void OnDispose() { }


        protected string Ctx(string message) => $"[Node '{Name}'] {message}";
    }
}
