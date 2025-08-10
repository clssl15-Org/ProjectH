using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure
{
    public partial class Work
    {
        public class HierarchyManager : IDisposable
        {
            // Front
            public bool Active { get; private set; } = false;

            public Work Parent { get; private set; }
            public Work CurrentChild { get; private set; }

            public string ReservedChild { get; private set; } = string.Empty;
            private object[] reservedArgs = null;

            public string PrimaryChild { get; set; } = string.Empty;

            // Control
            public bool CanOpen => Parent?.Active ?? true;

            // Internal
            private readonly Work owner;
            private readonly Dictionary<string, Work> children = new();


            // Content
            public HierarchyManager(Work owner) => this.owner = owner;

            public void Open(params object[] args)
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
                CurrentChild?.Open(args);
            }

            public void Invoke()
            {
                if (!Active) return;
                CurrentChild?.Invoke();
            }

            public void Close()
            {
                if (!Active) return;
                Active = false;

                var currentChild = CurrentChild;
                CurrentChild = null;

                currentChild?.Close();
            }

            public void SetNext(string next, bool restartIfPossible, params object[] args)
            {
                if (string.IsNullOrWhiteSpace(next))
                {
                    ClearNext();
                    Close();

                    return;
                }


                if (!children.ContainsKey(next))
                    throw new ArgumentException($"[Work-Hierarchy: {owner.Name}]: No child with the name '{next}' exists.");

                if (Active && next == CurrentChild?.Name && !restartIfPossible)
                    return;

                ReservedChild = next;
                reservedArgs = args;

                if (!Active) return;

                Close();
                Open();
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
                if (children.ContainsKey(work.Name))
                    throw new ArgumentException($"[Work: {owner.Name}]: A child with the name '{work.Name}' already exists.");
                if (owner == work)
                    throw new ArgumentException($"[Work: {owner.Name}]: A work cannot be its own child.");

                work.hierarchyManager.Parent = owner;
                children.Add(work.Name, work);

                if (primary) SetPrimary(work.Name);
                return work;
            }

            public Work RemoveChild(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException($"[Work: {owner.Name}]: The name cannot be empty.", nameof(name));
                if (!children.ContainsKey(name))
                    throw new ArgumentException($"[Work: {owner.Name}]: Cannot remove child '{name}' because it does not exist.", nameof(name));

                var work = children[name];
                work.Close();

                if (ReservedChild == name) ReservedChild = string.Empty;
                if (PrimaryChild == name) PrimaryChild = string.Empty;

                work.hierarchyManager.Parent = null;
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
                    throw new ArgumentException($"[Work: {owner.Name}]: No child with the name '{name}' exists.");

                PrimaryChild = name;
            }
            #endregion

            public void Dispose()
            {
                Close();
                ClearNext();

                foreach (var child in children.Values.ToList())
                    child.Dispose();
            }
        }
    }
}
