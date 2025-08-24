//using System;

//namespace UniEngine.StateMachines.BT
//{
//    /// <summary>
//    /// Provides basic state overrides.
//    /// </summary>
//    /// <remarks>
//    /// Additional decorator capabilities will be added in future revisions.
//    /// </remarks>
//    public class Decorator<TOwner, TBlackboard> : IBTNodeInternal<TOwner, TBlackboard> where TOwner : class where TBlackboard : class
//    {
//        // Front
//        public string Name
//        {
//            get => node != null ? $"{_name}: {node.Name}" : _name;
//            set => _name = value;
//        }

//        public IBTNode<TOwner, TBlackboard> Parent { get; private set; }

//        public TOwner Owner => node?.Owner;
//        public TBlackboard Blackboard => node?.Blackboard;

//        public bool OverrideAbortPolicy { get; set; } = false;
//        public AbortPolicies AbortPolicy
//        {
//            get => OverrideAbortPolicy ? _abortPolicy : (node?.AbortPolicy ?? AbortPolicies.None);
//            set
//            {
//                OverrideAbortPolicy = true;
//                _abortPolicy = value;
//            }
//        }

//        public bool OverrideLoop { get; set; } = false;
//        public LoopType LoopType
//        {
//            get => (!OverrideLoop && node != null) ? node.LoopType : _loop;
//            set
//            {
//                OverrideLoop = true;
//                _loop = value;

//                if (node != null)
//                    node.LoopType = value;
//            }
//        }

//        public bool OverrideNodeStatus { get; set; } = false;

//        public bool IsRunning => DetailedNodeStatus == DetailedNodeStatus.Running;
//        public NodeStatus NodeStatus => DetailedNodeStatus.ToNodeStatus();
//        public DetailedNodeStatus DetailedNodeStatus
//        {
//            get => (!OverrideNodeStatus && node != null) ? node.DetailedNodeStatus : _detailedNodeStatus;

//            set
//            {
//                OverrideNodeStatus = true;
//                _detailedNodeStatus = value;
//            }
//        }

//        public Func<bool> CheckCondition
//        {
//            get
//            {
//                if (_checkCondition != null)
//                    return _checkCondition;

//                if (node != null)
//                    return node.CheckCondition;

//                return DefaultCheckCondition;
//            }
//            set => _checkCondition = value;
//        }

//        bool IBTNodeInternal<TOwner, TBlackboard>.RetickNow
//        {
//            get => node?.RetickNow ?? false;
//            set
//            {
//                if (node != null)
//                    node.RetickNow = value;
//            }
//        }

//        IBTNodeInternal<TOwner, TBlackboard> IBTNodeInternal<TOwner, TBlackboard>.CurrentChild => node;
//        public bool IsDisposed { get; private set; } = false;


//        // Internal
//        private string _name = "Decorator";
//        private IBTNodeInternal<TOwner, TBlackboard> node;

//        private AbortPolicies _abortPolicy = AbortPolicies.None;

//        private LoopType _loop = LoopType.None;
//        private LoopType originalLoopType;

//        private DetailedNodeStatus _detailedNodeStatus;
//        private Func<bool> _checkCondition;


//        // Content
//        public Decorator(IBTNode<TOwner, TBlackboard> node = null, string name = null)
//        {
//            if (name != null) _name = name;
//            if (node != null) SetChild(node);
//        }

//        public Decorator<TOwner, TBlackboard> SetChild(IBTNode<TOwner, TBlackboard> node)
//        {
//            ThrowIfDisposed();

//            if (node == null)
//                throw new ArgumentNullException(nameof(node), Ctx("Cannot add null node"));
//            if (node.IsDisposed)
//                throw new ArgumentException(Ctx($"Cannot add node '{node.Name}' as a child because it has already been disposed."), nameof(node));
//            if (node is not IBTNodeInternal<TOwner, TBlackboard> _node)
//                throw new ArgumentException(Ctx($"Child node must be of type '{typeof(IBTNodeInternal<TOwner, TBlackboard>).Name}' or its derivative type."), nameof(node));
//            if (_node.Parent != null)
//                throw new InvalidOperationException(Ctx($"Child '{node.Name}' already has a parent ({node.Parent.Name})."));
//            if (this.node != null)
//                throw new InvalidOperationException(Ctx($"Cannot add node '{node.Name}' as a child because this decorator already has a child node '{this.node.Name}'."));

//            originalLoopType = node.LoopType;
//            if (OverrideLoop) node.LoopType = _loop;

//            _node.SetParent(this);
//            this.node = _node;

//            return this;
//        }
//        public Decorator<TOwner, TBlackboard> RemoveChild()
//        {
//            if (node == null)
//                return this;

//            node.Halt();
//            node.LoopType = originalLoopType;

//            node.SetParent(null);
//            node = null;

//            return this;
//        }


//        void IBTNodeInternal<TOwner, TBlackboard>.SetParent(IBTNodeInternal<TOwner, TBlackboard> parent) => Parent = parent;

//        bool IBTNodeInternal<TOwner, TBlackboard>.CheckCondition() => CheckCondition();
//        protected virtual bool DefaultCheckCondition()
//        {
//            ThrowIfDisposed();
//            return true;
//        }

//        public void Halt() => Halt(DetailedNodeStatus.HaltedExternal);
//        void IBTNodeInternal<TOwner, TBlackboard>.Halt(DetailedNodeStatus reason) => Halt(reason);
//        private void Halt(DetailedNodeStatus reason)
//        {
//            if (IsDisposed)
//                return;

//            node?.Halt(reason);
//        }

//        public void Tick()
//        {
//            ThrowIfDisposed();
//            node?.Tick();
//        }


//        private void ThrowIfDisposed()
//        {
//            if (IsDisposed)
//                throw new ObjectDisposedException(GetType().Name, Ctx("This decorator has been disposed."));
//        }

//        public void Dispose()
//        {
//            if (IsDisposed)
//                return;

//            var child = node;
//            if (child != null)
//            {
//                RemoveChild();
//                child.Dispose();
//            }

//            Parent = null;

//            _detailedNodeStatus = DetailedNodeStatus.Disposed;
//            IsDisposed = true;
//        }


//        protected string Ctx(string message) => $"[Decorator '{Name}'] {message}";
//    }
//}
