using System;
using System.Linq;
using UnityEngine;

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

        private readonly HierarchyManager hierarchyManager;
        //private readonly StreamManager streamManager;

        // Internal
        protected Action start;
        protected Action update;
        protected Action stop;
        private readonly Action[] none = Array.Empty<Action>();

        private bool _active = false;
        private object currentToken = null;


        // Content
        public Work(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"The name cannot be empty.", nameof(name));
            
            Name = name;
            hierarchyManager = new(this);
            //streamManager = new(this);
        }

        public virtual void Open(params object[] args)
        {
            ThrowIfDisposed();
            if (!hierarchyManager.CanOpen)
            {
                Debug.LogWarning($"Cannot activate because parent is not active.");
                return;
            }

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

            //if (hierarchyManager.Parent == null)
            //    streamManager.Start();

            currentToken = null;
        }
        protected virtual void Start(params object[] args) { }


        public virtual void Invoke()
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


        public virtual void Close()
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

            //streamManager.Stop();


            foreach (Action action in OnStopped?.GetInvocationList()?.ToArray() ?? none)
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            currentToken = null;
        }
        protected virtual void Stop() { }

        private bool CheckToken(object token) => token == currentToken;


        public void SetNext(string next, bool restartIfPossible = false)
        {
            ThrowIfDisposed();
            hierarchyManager.SetNext(next, restartIfPossible);
        }
        public void SetNextWith(string next, params object[] args)
        {
            ThrowIfDisposed();
            hierarchyManager.SetNext(next, true, args);
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
        public Work AddChild(string name, bool primary = false)
        {
            ThrowIfDisposed();
            return hierarchyManager.AddChild(name, primary);
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
        public Work RemoveChild(string name)
        {
            ThrowIfDisposed();
            return hierarchyManager.RemoveChild(name);
        }

        /// <summary>
        /// Removes the child with the specified name from this instance,
        /// and returns the parent (this object) for method chaining.
        /// </summary>
        /// <param name="name">The name of the child to remove.</param>
        /// <returns>This <see cref="Work"/> instance (the parent), enabling chained calls.</returns>
        public Work Delete(string name)
        {
            ThrowIfDisposed();
            hierarchyManager.RemoveChild(name);
            return this;
        }

        /// <summary>
        /// Marks the child with the specified name as the primary child of this instance,
        /// returning the parent (this object) for method chaining.
        /// </summary>
        /// <param name="name">The name of the child to mark as primary.</param>
        /// <returns>This <see cref="Work"/> instance (the parent), enabling chained calls.</returns>
        public Work SetPrimary(string name)
        {
            ThrowIfDisposed();
            hierarchyManager.SetPrimary(name);
            return this;
        }
        #endregion

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

            //streamManager.Dispose();
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
