using System;
using System.Collections.Generic;
using System.Linq;

namespace UniEngine.StateMachines.FSM
{
    public partial class Work
    {
        protected class HierarchyManager : IDisposable
        {
            // Front
            public bool Active { get; private set; } = false;

            public Work Parent
            {
                get => _parent;
                private set
                {
                    ThrowIfActive();
                    _parent = value;
                }
            }
            public Work CurrentChild { get; private set; }

            public string ReservedChild { get; private set; } = string.Empty;
            private object[] reservedArgs = null;

            public string PrimaryChild { get; set; } = string.Empty;

            // Control
            public bool CanOpen => Parent?.Active ?? true;

            // Internal
            private readonly Work ownerWork;
            private Work _parent;
            private readonly Dictionary<string, Work> children = new();

            private bool isDisposed = false;


            // Content
            public HierarchyManager(Work owner) => this.ownerWork = owner;

            public void Enter(params object[] args)
            {
                if (Active) return;
                Active = true;

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
                if (!Active) return;
                CurrentChild?.Update();
            }

            public void Exit()
            {
                if (!Active) return;
                Active = false;

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

                if (Active && next == CurrentChild?.Name && !restartIfPossible)
                    return;

                ReservedChild = next;
                reservedArgs = args;

                if (!Active) return;

                Exit();
                Enter();
            }

            public void ClearNext()
            {
                ReservedChild = string.Empty;
                reservedArgs = null;
            }


            #region Child Management
            public Work AddChild(string name, bool primary = false) => AddChild(new Work(name), primary);
            public T AddChild<T>(T work, bool primary = false) where T : Work
            {
                ThrowIfActive();

                if (children.ContainsKey(work.Name))
                    throw new ArgumentException(Ctx($"A child named '{work.Name}' already exists."), nameof(work));
                if (ownerWork == work)
                    throw new ArgumentException(Ctx("A work cannot be its own child."), nameof(work));
                if (work.hierarchy.Parent != null)
                    throw new InvalidOperationException(Ctx($"Child '{work.Name}' already has a parent ({work.hierarchy.Parent.Name})."));

                work.hierarchy.Parent = ownerWork;
                children.Add(work.Name, work);

                if (primary) SetPrimary(work.Name);
                return work;
            }

            public Work RemoveChild(string name)
            {
                ThrowIfActive();

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException(Ctx("Name cannot be null or whitespace."), nameof(name));
                if (!children.ContainsKey(name))
                    throw new ArgumentException(Ctx($"This work does not contain a child named '{name}'."), nameof(name));

                var work = children[name];
                work.Exit();

                if (CurrentChild == work) CurrentChild = null;
                if (ReservedChild == name) ReservedChild = string.Empty;
                if (PrimaryChild == name) PrimaryChild = string.Empty;

                work.hierarchy.Parent = null;
                children.Remove(name);

                return work;
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


            private string Ctx(string message) => ownerWork.Ctx(message);
        }
    }
}
