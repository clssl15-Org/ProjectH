using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure
{
    public partial class Work : IDisposable
    {
        // Front
        public string Name { get; }

        public bool Active
        {
            get => _active;
            set
            {
                if (value) Open();
                else Close();
            }
        }

        public bool IsDisposed { get; private set; } = false;
        private bool isDisposing = false;

        // Property
        public event Action OnStarted;
        public event Action OnUpdated;
        public event Action OnStopping;
        public event Action OnStopped;

        protected readonly HierarchyManager hierarchyManager;


        // Internal
        protected Action start;
        protected Action update;
        protected Action stop;
        private readonly Action[] none = Array.Empty<Action>();

        private bool _active = false;
        private object currentToken = null;


        // Content
        public Work()
        {
            Name = GetType().Name;
            hierarchyManager = new(this);
        }
        public Work(object name)
        {
            if (name is null)
                throw new ArgumentException($"The name cannot be null.", nameof(name));
            
            Name = GetName(name);
            hierarchyManager = new(this);
        }

        public void Open(params object[] args)
        {
            ThrowIfDisposed();

            if (!hierarchyManager.CanOpen)
                throw new InvalidOperationException($"Cannot activate Work '{Name}' because parent is not active.");

            if (_active) return;
            _active = true;

            var token = new object();
            currentToken = token;

            Start(args);
            if (!CheckToken(token)) return;

            foreach (Action action in start?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            foreach (Action action in OnStarted?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            hierarchyManager.Open();
            if (!CheckToken(token)) return;

            currentToken = null;
        }
        protected virtual void Start(params object[] args) { }


        public void Invoke()
        {
            ThrowIfDisposed();
            if (!_active) return;

            var token = new object();
            currentToken = token;

            Update();
            if (!CheckToken(token)) return;

            foreach (Action action in update?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            foreach (Action action in OnUpdated?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            hierarchyManager.Invoke();
            if (!CheckToken(token)) return;

            currentToken = null;
        }
        protected virtual void Update() { }


        public void Close()
        {
            if (!_active) return;
            _active = false;

            var token = new object();
            currentToken = token;


            foreach (Action action in OnStopping?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            hierarchyManager.Close();
            if (!CheckToken(token)) return;

            foreach (Action action in stop?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            Stop();
            if (!CheckToken(token)) return;


            foreach (Action action in OnStopped?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            currentToken = null;
        }
        protected virtual void Stop() { }

        private bool CheckToken(object token) => token == currentToken;


        public void SetNext(object next, bool restartIfPossible = false)
        {
            ThrowIfDisposed();
            hierarchyManager.SetNext(GetName(next), restartIfPossible);
        }
        public void SetNextWith(object next, params object[] args)
        {
            ThrowIfDisposed();
            hierarchyManager.SetNext(GetName(next), true, args);
        }
        public void ClearNext()
        {
            ThrowIfDisposed();
            hierarchyManager.ClearNext();
        }

        public Work SetStartAction(Action action)
        {
            start = action;
            return this;
        }
        public Work SetUpdateAction(Action action)
        {
            update = action;
            return this;
        }
        public Work SetStopAction(Action action)
        {
            stop = action;
            return this;
        }
        public Work SetActive(bool active)
        {
            Active = active;
            return this;
        }


        #region Child Management
        /// <summary>
        /// Creates and adds a new child <see cref="Work"/> with the specified name,
        /// and returns the newly created child.
        /// </summary>
        /// <param name="name">The name of the new child to create.</param>
        /// <param name="primary">Indicates whether this new child is considered primary.</param>
        /// <returns>The newly created child <see cref="Work"/>.</returns>
        public Work AddChild(object name, bool primary = false)
        {
            ThrowIfDisposed();
            return hierarchyManager.AddChild(GetName(name), primary);
        }

        /// <summary>
        /// Attaches an existing <see cref="Work"/> object to this instance,
        /// and returns the attached child.
        /// </summary>
        /// <param name="work">The existing child instance to attach.</param>
        /// <param name="primary">Indicates whether this child is considered primary.</param>
        /// <returns>The attached child of type <typeparamref name="T"/>.</returns>
        public T AddChild<T>(T work, bool primary = false) where T : Work
        {
            ThrowIfDisposed();
            return hierarchyManager.AddChild(work, primary);
        }

        /// <summary>
        /// Attaches an existing <see cref="Work"/> object to this instance,
        /// and returns the parent (this object) for method chaining.
        /// </summary>
        /// <param name="work">The existing child instance to attach.</param>
        /// <param name="primary">Indicates whether this child is considered primary.</param>
        /// <returns>This <see cref="Work"/> instance (the parent), enabling chained calls.</returns>
        public Work Append<T>(T work, bool primary = false) where T : Work
        {
            ThrowIfDisposed();
            hierarchyManager.AddChild(work, primary);
            return this;
        }

        /// <summary>
        /// Removes the child with the specified name from this instance,
        /// and returns the removed child <see cref="Work"/>.
        /// </summary>
        /// <param name="name">The name of the child to remove.</param>
        /// <returns>The removed child <see cref="Work"/>.</returns>
        public Work RemoveChild(object name)
        {
            ThrowIfDisposed();
            return hierarchyManager.RemoveChild(GetName(name));
        }

        /// <summary>
        /// Removes the child with the specified name from this instance,
        /// and returns the parent (this object) for method chaining.
        /// </summary>
        /// <param name="name">The name of the child to remove.</param>
        /// <returns>This <see cref="Work"/> instance (the parent), enabling chained calls.</returns>
        public Work Delete(object name)
        {
            ThrowIfDisposed();
            hierarchyManager.RemoveChild(GetName(name));
            return this;
        }

        /// <summary>
        /// Marks the child with the specified name as the primary child of this instance,
        /// returning the parent (this object) for method chaining.
        /// </summary>
        /// <param name="name">The name of the child to mark as primary.</param>
        /// <returns>This <see cref="Work"/> instance (the parent), enabling chained calls.</returns>
        public Work SetPrimary(object name)
        {
            ThrowIfDisposed();
            hierarchyManager.SetPrimary(GetName(name));
            return this;
        }

        /// <summary>
        /// Attempts to retrieve the current child of the specified type.
        /// </summary>
        public bool TryGetCurrentChild<T>(out T current) where T : Work
        {
            current = null;

            if (hierarchyManager.CurrentChild is null)
                return false;
            if (hierarchyManager.CurrentChild is not T _current)
                return false;

            current = _current;
            return true;
        }
        #endregion


        public string GetFullState()
        {
            var logs = new List<string>();
            var current = this;

            do
            {
                logs.Add(current.Name);
            } while (current.TryGetCurrentChild(out current));

            return string.Join(" - ", logs);
        }

        private string GetName(object source) => source switch
        {
            null => string.Empty,
            string name => name,
            Type type => type.Name,
            Work work => work.Name,
            _ => source.ToString()
        };

        void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name, $"[Work: {Name}] : Work has been disposed.");
        }

        public void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;

            Close();
            hierarchyManager.Dispose();

            IsDisposed = true;
        }



        public static implicit operator string(Work work) => work?.Name ?? "null";

        public override string ToString()
        {
            return $"{Name}, Active: {Active}" +
                $"Current: {hierarchyManager.CurrentChild?.Name ?? "None"}\n" +
                $"Primary: {(!string.IsNullOrWhiteSpace(hierarchyManager.PrimaryChild) ? hierarchyManager.PrimaryChild : "None")}\n" +
                $"Reserved: {(!string.IsNullOrWhiteSpace(hierarchyManager.ReservedChild) ? hierarchyManager.ReservedChild : "None")}";
        }
    }
}
