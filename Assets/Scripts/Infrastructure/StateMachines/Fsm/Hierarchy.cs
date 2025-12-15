using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.StateMachines.Fsm
{
    public partial class Work
    {
        protected class HierarchyManager : IDisposable
        {
            // Front
            public Work Parent
            {
                get => _parent;
                private set
                {
                    ThrowIfActive();
                    _parent = value;
                }
            }
            public IChildWork CurrentChild { get; private set; }

            public string ReservedChild { get; private set; } = string.Empty;
            private object[] reservedArgs = null;

            public string PrimaryChild { get; set; } = string.Empty;

            // Control
            public bool CanOpen => Parent?.Active ?? true;
            public readonly IReadOnlyDictionary<string, IChildWork> Children;

            // Internal
            private readonly Work ownerWork;
            private Work _parent;
            private readonly Dictionary<string, IChildWork> children;

            private bool isDisposed = false;


            // Content
            public HierarchyManager(Work owner)
            {
                ownerWork = owner;

                children = new();
                Children = children;
            }

            public void Enter(params object[] args)
            {
                if (!string.IsNullOrWhiteSpace(ReservedChild))
                {
                    CurrentChild = children[ReservedChild];
                    if (args == null || args.Length == 0) args = reservedArgs;
                }
                else if (!string.IsNullOrWhiteSpace(PrimaryChild))
                {
                    CurrentChild = children[PrimaryChild];
                }

                ClearNext();
                CurrentChild?.Enter(args);
            }

            public void Update()
            {
                CurrentChild?.Update();
            }

            public void Exit()
            {
                var currentChild = CurrentChild;
                CurrentChild = null;

                currentChild?.Exit();
            }

            public void SetNext(string next, bool restartIfPossible, params object[] args)
            {
                if (string.IsNullOrWhiteSpace(next))
                {
                    ClearNext();
                    Exit();

                    return;
                }


                if (!children.ContainsKey(next))
                    throw new ArgumentException(Ctx($"No child named '{next}' exists."), nameof(next));

                if (next == CurrentChild?.Name && !restartIfPossible)
                    return;

                if (!ownerWork.Active) return;

                CurrentChild?.Exit();

                CurrentChild = children[next];
                CurrentChild.Enter(args);
            }

            public void SetNextToNone() => SetNext(null, false);

            public void ClearNext()
            {
                ReservedChild = string.Empty;
                reservedArgs = null;
            }


            #region Child Management
            public T AddChild<T>(string name, T child, bool primary = false) where T : class, IChildWork
            {
                ThrowIfActive();
                var childWork = child as Work;

                if (children.ContainsKey(name))
                    throw new ArgumentException(Ctx($"A child with name '{name}' already exists."), nameof(child));
                if (ownerWork == child)
                    throw new ArgumentException(Ctx("A work cannot be its own child."), nameof(child));
                if (childWork != null && childWork.hierarchy.Parent != null)
                    throw new InvalidOperationException(Ctx($"Child '{name}' already has a parent ({childWork.hierarchy.Parent.Name})."));

                if (childWork != null)
                    childWork.hierarchy.Parent = ownerWork;

                children.Add(name, child);

                if (primary) SetPrimary(name);
                return child;
            }

            public IChildWork RemoveChild(string name)
            {
                ThrowIfActive();

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException(Ctx("Name cannot be null or whitespace."), nameof(name));
                if (!children.ContainsKey(name))
                    throw new ArgumentException(Ctx($"This work does not contain a child named '{name}'."), nameof(name));

                var child = children[name];
                var childWork = child as Work;
                child.Exit();

                if (CurrentChild == child) CurrentChild = null;
                if (ReservedChild == name) ReservedChild = string.Empty;
                if (PrimaryChild == name) PrimaryChild = string.Empty;

                if (childWork != null)
                    childWork.hierarchy.Parent = null;

                children.Remove(name);

                return child;
            }

            public void SetPrimary(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    PrimaryChild = string.Empty;
                    return;
                }

                if (!children.ContainsKey(name))
                    throw new ArgumentException(Ctx($"No child named '{name}' exists."), nameof(name));

                PrimaryChild = name;
            }
            #endregion

            private void ThrowIfActive()
            {
                if (ownerWork.Active)
                    throw new InvalidOperationException(Ctx("Cannot change the hierarchy while it is active."));
            }

            public void Dispose()
            {
                if (isDisposed) return;
                isDisposed = true;

                Exit();
                ClearNext();

                foreach (var child in children.Values.ToList())
                    child.Dispose();
            }

            private string Ctx(string message) => ownerWork.FormatLogMessage(message);
        }
    }
}
