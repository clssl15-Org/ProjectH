using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static UniEngine.StateMachines.Tools;

namespace UniEngine.StateMachines.BT
{
    public partial class BTNode<TOwner, TBlackboard> : IBTNodeInternal<TOwner, TBlackboard> where TOwner : class where TBlackboard : class, new()
    {
        // Front
        public string Name { get; }

        public IBTNode<TOwner, TBlackboard> ParentNode { get; private set; }

        public TOwner Owner => ParentNode?.Owner ?? _owner;
        public TBlackboard Blackboard => ParentNode?.Blackboard ?? (_blackboard ??= CreateBlackboard());

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
            get => HierarchyComponent.HierarchyMode;
            set => HierarchyComponent.HierarchyMode = value;
        }
        public LoopType LoopType
        {
            get => HierarchyComponent.LoopType;
            set => HierarchyComponent.LoopType = value;
        }

        public bool IsRunning => DetailedNodeStatus == DetailedNodeStatus.Running;
        public NodeStatus NodeStatus => DetailedNodeStatus.ToNodeStatus();
        public DetailedNodeStatus DetailedNodeStatus { get; private set; } = DetailedNodeStatus.NeverRun;

        public bool IsSelectable { get; protected set; } = true;

        IBTNodeInternal<TOwner, TBlackboard> IBTNodeInternal<TOwner, TBlackboard>.CurrentChild => _hierarchyComponent?.CurrentChild;
        public bool IsDisposed { get; private set; } = false;

        // Property
        private Hierarchy HierarchyComponent => _hierarchyComponent ??= new(this);
        private Hierarchy _hierarchyComponent;

        protected bool RetickNow
        {
            get => ((IBTNodeInternal<TOwner, TBlackboard>)this).RetickNow;
            set => ((IBTNodeInternal<TOwner, TBlackboard>)this).RetickNow = value;
        }
        bool IBTNodeInternal<TOwner, TBlackboard>.RetickNow { get; set; } = false;

        // Internal
        private readonly TOwner _owner;
        private TBlackboard _blackboard;
        private bool isDisposing = false;


        // Content
        public BTNode(TOwner owner = null, string name = null)
        {
            _owner = owner;
            Name = name ?? GetName(GetType());
        }
        protected virtual TBlackboard CreateBlackboard() => new();

        object IBTNode.GetOwner() => Owner;
        object IBTNode.GetBlackboard() => Blackboard;

        protected virtual bool CheckCondition()
        {
            try
            {
                ThrowIfDisposed();
                return true;
            }
            catch (Exception ex)
            {
#if UNIENGINE
                ExceptionHandler.Report(ex);
#else
                UnityEngine.Debug.LogException(ex);
#endif

                return false;
            }
        }
        bool IBTNodeInternal<TOwner, TBlackboard>.CheckCondition() => CheckCondition();


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

                _hierarchyComponent?.Tick();
            }
            catch (Exception ex)
            {
                Halt(DetailedNodeStatus.Failure);

#if UNIENGINE
                ExceptionHandler.Report(ex);
#else
                UnityEngine.Debug.LogException(ex);
#endif
            }
        }

        protected virtual void Inject(object[] inputs) { }

        protected virtual void OnOpen(params object[] inputs) { }
        protected virtual void OnTick() { }

        /// <summary>
        /// Marks the node as successfully completed.
        /// </summary>
        /// <remarks>
        /// Equivalent to calling <see cref="Complete(bool)"/> with <c>true</c>.
        /// Use this from within the node; external callers should prefer <see cref="Halt()"/>.
        /// </remarks>
        protected void Complete() => Complete(true);
        /// <summary>
        /// Marks the node as complete with an explicit result.
        /// </summary>
        protected void Complete(bool succeed)
        {
            Halt(succeed ? DetailedNodeStatus.Success : DetailedNodeStatus.Failure);
        }

        public void Halt() => Halt(DetailedNodeStatus.HaltedExternal);
        void IBTNodeInternal<TOwner, TBlackboard>.Halt(DetailedNodeStatus reason) => Halt(reason);
        private void Halt(DetailedNodeStatus reason)
        {
            if (IsDisposed) return;
            if (!IsRunning) return;

            _hierarchyComponent?.Halt();
            OnHalt(reason);

            DetailedNodeStatus = reason;
        }
        protected virtual void OnHalt(DetailedNodeStatus reason) { }


        #region Select Child
        public void SelectChild(params SelectionRequest[] requests) => SelectChild((IEnumerable<SelectionRequest>)requests);
        public void SelectChild(IEnumerable<SelectionRequest> requests)
        {
            SelectionRequest.ThrowIfNullOrEmpty(requests, Ctx);

            var self = (IBTNodeInternal<TOwner, TBlackboard>)this;

            if (self.CheckSelectionCondition(requests.First()) != SelectionResult.Selected)
                return;

            self.SelectChildInternal(requests);
        }

        public SelectionResult CheckSelectionCondition(SelectionRequest request)
        {
            if (request.Predicate == null)
                throw new ArgumentNullException(nameof(request.Predicate), Ctx("Predicate cannot be null."));

            if (!request.Predicate(this))
                return SelectionResult.NotMatched;


            var willOpen = !IsRunning || request.RerunPolicy == RerunPolicy.Restart;

            var shouldCheckCondition = request.EntryPolicy == EntryPolicy.CheckAlways
                || (request.EntryPolicy == EntryPolicy.CheckOnOpen && willOpen);

            if (shouldCheckCondition && !CheckCondition())
                return SelectionResult.ConditionFailure;

            return SelectionResult.Selected;
        }

        void IBTNodeInternal<TOwner, TBlackboard>.SelectChildInternal(IEnumerable<SelectionRequest> requests)
        {
            SelectionRequest.ThrowIfNullOrEmpty(requests, Ctx);
            var request = requests.First();


            if (IsRunning)
            {
                switch (request.RerunPolicy)
                {
                    case RerunPolicy.IgnoreIfRunning:
                        break;

                    case RerunPolicy.Restart:
                        Halt(DetailedNodeStatus.HaltedExternal);

                        DetailedNodeStatus = DetailedNodeStatus.Running;
                        OnOpen(request.Inputs);
                        break;

                    case RerunPolicy.EnsureRunningAndInjectInputs:
                        Inject(request.Inputs);
                        break;
                }
            }
            else
            {
                DetailedNodeStatus = DetailedNodeStatus.Running;

                if (request.RerunPolicy == RerunPolicy.EnsureRunningAndInjectInputs)
                {
                    // Do not invoke OnOpen to bypass common initialization.
                    Inject(request.Inputs);
                }
                else
                {
                    OnOpen(request.Inputs);
                }
            }


            var _requests = requests.Skip(1);
            if (_requests.Any()) HierarchyComponent.SelectChild(_requests);
        }
        #endregion


        #region Child Management
        public BTNode<TOwner, TBlackboard> AddChild(IBTNode<TOwner, TBlackboard> child)
        {
            ThrowIfDisposed();
            HierarchyComponent.AddChild(child);

            return this;
        }

        public BTNode<TOwner, TBlackboard> RemoveChild(object name)
        {
            ThrowIfDisposed();
            HierarchyComponent.RemoveChild(GetName(name));

            return this;
        }
        public BTNode<TOwner, TBlackboard> RemoveChild(Predicate<IBTNode> predicate)
        {
            ThrowIfDisposed();
            HierarchyComponent.RemoveChild(predicate);

            return this;
        }
        #endregion

        void IBTNodeInternal<TOwner, TBlackboard>.SetParent(IBTNodeInternal<TOwner, TBlackboard> parent) => ParentNode = parent;


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
            _hierarchyComponent?.Dispose();

            ParentNode = null;
            IsDisposed = true;
        }
        protected virtual void OnDispose() { }


        protected string Ctx(string message) => $"[Node '{Name}'] {message}";

        public string GetFullState()
        {
            var sb = new StringBuilder();

            IBTNodeInternal<TOwner, TBlackboard> current = this;
            sb.Append($"[{current.Name}:{GetStatusId(current)}]");

            while (true)
            {
                current = current.CurrentChild;
                if (current == null) break;

                sb.Append($" - [{current.Name}:{GetStatusId(current)}]");
            }

            string GetStatusId(IBTNode node) => node.NodeStatus switch
            {
                NodeStatus.NeverRun => "N",
                NodeStatus.Running => "R",
                NodeStatus.Success => "S",
                NodeStatus.Failure => "F",
                NodeStatus.Aborted => "A",
                _ => "?",
            };

            return sb.ToString();
        }
    }
}
