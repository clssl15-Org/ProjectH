using System;
using System.Collections.Generic;
using System.Linq;

namespace UniEngine.StateMachines.BT
{
    public partial class BTNode
    {
        private partial class HierarchyManager : IDisposable
        {
            // Front
            public HierarchyMode HierarchyMode
            {
                get => _hierarchyMode;
                set
                {
                    ThrowIfDisposed();
                    ThrowIfRunning();

                    if (_hierarchyMode == value)
                        return;

                    _hierarchyMode = value;

                    hierarchyComponent = value switch
                    {
                        HierarchyMode.Selector => new Selector(this),
                        HierarchyMode.Sequence => new Sequence(this),
                        _ => null
                    };
                }
            }

            public LoopType LoopType
            {
                get => _loopType;
                set
                {
                    ThrowIfDisposed();
                    ThrowIfRunning();

                    _loopType = value;
                }
            }

            protected IReadOnlyList<IBTNodeInternal> Children { get; }
            public bool IsDisposed { get; private set; } = false;

            // Internal
            private readonly BTNode OwnerNode;

            private interface IHierarchyComponent
            {
                bool ReadUpper(IBTNodeInternal child);
                bool ReadCurrent(IBTNodeInternal child);
                bool ReadLower(IBTNodeInternal child);
            }

            private IHierarchyComponent hierarchyComponent;
            private HierarchyMode _hierarchyMode = HierarchyMode.None;

            private readonly List<IBTNodeInternal> children;
            protected IBTNodeInternal CurrentChild { get; set; }

            private LoopType _loopType = LoopType.None;
            private bool isDisposing = false;


            // Content
            public HierarchyManager(BTNode ownerNode)
            {
                OwnerNode = ownerNode;

                children = new();
                Children = children.AsReadOnly();

                HierarchyMode = HierarchyMode.Selector;
            }

            public void Tick()
            {
                while (true)
                {
                    ThrowIfDisposed();

                    if (hierarchyComponent == null || children.Count == 0)
                        return;


                    var currentIndex = CurrentChild != null ? children.IndexOf(CurrentChild) : -1;
                    int i = -1;

                    foreach (var child in children)
                    {
                        i++;

                        if (i < currentIndex)
                        {
                            if (!hierarchyComponent.ReadUpper(child))
                                break;
                        }
                        else if (i == currentIndex)
                        {
                            if (!hierarchyComponent.ReadCurrent(child))
                                break;
                        }
                        else
                        {
                            if (!hierarchyComponent.ReadLower(child))
                                break;
                        }
                    }

                    if (CurrentChild != null)
                    {
                        CurrentChild.Tick();

                        var retickNow = CurrentChild.RetickNow;
                        CurrentChild.RetickNow = false;

                        if (OwnerNode.IsRunning && retickNow)
                            continue;
                    }

                    break;
                }
            }
            protected void GetPolicy(IBTNode node, out bool self, out bool lowerPriority)
            {
#if UNIENGINE
                lowerPriority = ((int)node.AbortPolicy).HasAll((int)AbortPolicies.LowerPriority);
                self = ((int)node.AbortPolicy).HasAll((int)AbortPolicies.Self);
#else
                lowerPriority = node.AbortPolicy.HasFlag(AbortPolicies.LowerPriority);
                self = node.AbortPolicy.HasFlag(AbortPolicies.Self);
#endif
            }

            public void Halt()
            {
                CurrentChild?.Halt(DetailedNodeStatus.HaltedByParent);
                CurrentChild = null;
            }


            #region Child Management
            public void AddChild(IBTNode node, int index = -1)
            {
                ThrowIfDisposed();
                ThrowIfRunning();

                if (node == null)
                    throw new ArgumentNullException(nameof(node), Ctx("Cannot add null node"));
                if (node.IsDisposed)
                    throw new ArgumentException(Ctx($"Cannot add node '{node.Name}' as a child because it has already been disposed."), nameof(node));
                if (node is not IBTNodeInternal _node)
                    throw new ArgumentException(Ctx($"Child node must be of type '{typeof(IBTNodeInternal).Name}' or its derivative type."), nameof(node));
                if (children.Contains(_node))
                    throw new ArgumentException(Ctx($"This node '{node.Name}' is already a child."), nameof(node));
                if (children.Any(n => n.Name == node.Name))
                    throw new ArgumentException(Ctx($"A child named '{node.Name}' already exists."), nameof(node));
                if (OwnerNode == node)
                    throw new ArgumentException(Ctx("A node cannot be its own child."), nameof(node));
                if (node.Parent != null)
                    throw new InvalidOperationException(Ctx($"Child '{node.Name}' already has a parent ({node.Parent.Name})."));


                if (index >= 0)
                    children.Insert(index, _node);
                else
                    children.Add(_node);

                _node.Parent = OwnerNode;
            }

            public void RemoveChild(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException(Ctx("Name cannot be null or whitespace."), nameof(name));

                RemoveChild(c => c.Name == name);
            }
            public void RemoveChild(Predicate<IBTNode> predicate)
            {
                ThrowIfDisposed();
                ThrowIfRunning();

                var child = children.Find(predicate);
                if (child == null) return;

                child.Halt();

                if (child == CurrentChild)
                    CurrentChild = null;

                children.Remove(child);
                child.Parent = null;
            }
            #endregion


            private void ThrowIfRunning()
            {
                if (OwnerNode.IsRunning)
                    throw new InvalidOperationException(Ctx("Cannot modify children while this node is running."));
            }
            private void ThrowIfDisposed()
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name, Ctx("This hierarchy manager has been disposed."));
            }

            public void Dispose()
            {
                if (isDisposing) return;
                isDisposing = true;

                CurrentChild = null;

                children.ForEach(c => c.Dispose());
                children.Clear();

                HierarchyMode = HierarchyMode.None;
                IsDisposed = true;
            }


            private string Ctx(string message) => OwnerNode.Ctx(message);
        }
    }
}
