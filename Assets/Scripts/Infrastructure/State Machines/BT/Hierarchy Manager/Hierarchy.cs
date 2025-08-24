using System;
using System.Collections.Generic;
using System.Linq;

namespace UniEngine.StateMachines.BT
{
    public partial class BTNode<TOwner, TBlackboard>
    {
        private partial class Hierarchy : IDisposable
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

            protected IReadOnlyList<IBTNodeInternal<TOwner, TBlackboard>> Children { get; }
            public bool IsDisposed { get; private set; } = false;

            // Internal
            private readonly BTNode<TOwner, TBlackboard> OwnerNode;

            private interface IHierarchyComponent
            {
                bool ReadUpper(IBTNodeInternal<TOwner, TBlackboard> child);
                bool ReadCurrent(IBTNodeInternal<TOwner, TBlackboard> child);
                bool ReadLower(IBTNodeInternal<TOwner, TBlackboard> child);
            }

            private IHierarchyComponent hierarchyComponent;
            private HierarchyMode _hierarchyMode = HierarchyMode.None;

            private readonly List<IBTNodeInternal<TOwner, TBlackboard>> children;
            internal IBTNodeInternal<TOwner, TBlackboard> CurrentChild { get; private set; }

            private LoopType _loopType = LoopType.None;
            private bool isDisposing = false;


            // Content
            public Hierarchy(BTNode<TOwner, TBlackboard> ownerNode)
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
                            if (child.IsSelectable
                                && !hierarchyComponent.ReadUpper(child))
                                break;
                        }
                        else if (i == currentIndex)
                        {
                            if (!hierarchyComponent.ReadCurrent(child))
                                break;
                        }
                        else
                        {
                            if (child.IsSelectable
                                && !hierarchyComponent.ReadLower(child))
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
            protected void GetPolicy(IBTNode<TOwner, TBlackboard> node, out bool self, out bool lowerPriority)
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


            public void SelectChild(IEnumerable<SelectionRequest> requests)
            {
                SelectionRequest.ThrowIfNullOrEmpty(requests, Ctx);
                var request = requests.First();

                if (request.Predicate == null)
                    throw new ArgumentNullException(nameof(request.Predicate), Ctx("Predicate cannot be null."));


                foreach (var child in children)
                {
                    switch (child.CheckSelectionCondition(request))
                    {
                        case SelectionResult.NotMatched:
                            continue;

                        case SelectionResult.ConditionFailure:
                            return;

                        case SelectionResult.Selected:
                            {
                                if (CurrentChild != null && CurrentChild != child)
                                    CurrentChild.Halt(DetailedNodeStatus.HaltedByParent);

                                CurrentChild = child;
                                child.SelectChildInternal(requests);
                            }
                            return;

                        default:
                            throw new InvalidOperationException(Ctx($"Unknown SelectionResult type detected while checking '{child.Name}'"));
                    }
                }

                if (request.ThrowIfNotFound)
                    throw new InvalidOperationException(Ctx("Failed to find the corresponding Child.\n" +
                        $"Children: {string.Join(", ", Children.Select(c => c.Name))}"));
            }


            #region Child Management
            public void AddChild(IBTNode<TOwner, TBlackboard> node, int index = -1)
            {
                ThrowIfDisposed();
                ThrowIfRunning();

                if (node == null)
                    throw new ArgumentNullException(nameof(node), Ctx("Cannot add null node"));
                if (node.IsDisposed)
                    throw new ArgumentException(Ctx($"Cannot add node '{node.Name}' as a child because it has already been disposed."), nameof(node));
                if (node is not IBTNodeInternal<TOwner, TBlackboard> _node)
                    throw new ArgumentException(Ctx($"Child node must be of type '{typeof(IBTNodeInternal<TOwner, TBlackboard>).Name}' or its derivative type."), nameof(node));
                if (children.Contains(_node))
                    throw new ArgumentException(Ctx($"This node '{node.Name}' is already a child."), nameof(node));
                if (children.Any(n => n.Name == node.Name))
                    throw new ArgumentException(Ctx($"A child named '{node.Name}' already exists."), nameof(node));
                if (OwnerNode == node)
                    throw new ArgumentException(Ctx("A node cannot be its own child."), nameof(node));
                if (node.ParentNode != null)
                    throw new InvalidOperationException(Ctx($"Child '{node.Name}' already has a parent ({node.ParentNode.Name})."));


                if (index >= 0)
                    children.Insert(index, _node);
                else
                    children.Add(_node);

                _node.SetParent(OwnerNode);
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

                var child = children.FirstOrDefault(n => predicate((IBTNode)n));
                if (child == null) return;

                child.Halt();

                if (child == CurrentChild)
                    CurrentChild = null;

                children.Remove(child);
                child.SetParent(null);
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
                    throw new ObjectDisposedException(GetType().Name, Ctx("This hierarchy has been disposed."));
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


            private string Ctx(string message) => OwnerNode.Ctx($"(hierarchy) {message}");
        }
    }
}
