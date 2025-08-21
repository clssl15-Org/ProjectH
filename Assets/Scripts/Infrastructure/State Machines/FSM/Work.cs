using System;
using System.Collections.Generic;
using static UniEngine.StateMachines.Tools;

namespace UniEngine.StateMachines.FSM
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
                if (value) Enter();
                else Exit();
            }
        }

        public bool IsDisposed { get; private set; } = false;
        private bool isDisposing = false;

        // Property
        protected readonly HierarchyManager hierarchy;
#if UNIENGINE
        private readonly StreamManager stream;
#endif

        // Internal
        protected Action onEnter;
        protected Action<object[]> onEnterWith;
        protected Action onUpdate;
        protected Action onExit;

        private bool _active = false;
        private object currentToken = null;


        // Content
        public Work(string name = null)
        {
            Name = name ?? GetName(GetType());

            hierarchy = new(this);
#if UNIENGINE
            stream = new(this);
#endif
        }

        public void Enter(params object[] args)
        {
            ThrowIfDisposed();

            if (!hierarchy.CanOpen)
                throw new InvalidOperationException(Ctx("Cannot enter because the parent is not active."));

            if (_active) return;
            _active = true;

            var token = new object();
            currentToken = token;

            OnEnter(args);
            if (!CheckToken(token)) return;

            foreach (Action action in onEnter?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }
            foreach (Action<object[]> action in onEnterWith?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                action.Invoke(args);
                if (!CheckToken(token)) return;
            }

            hierarchy.Enter();
            if (!CheckToken(token)) return;

#if UNIENGINE
            if (hierarchy.Parent == null)
                stream.Enter();
#endif

            currentToken = null;
        }
        protected virtual void OnEnter(params object[] args) { }


        public void Update()
        {
            ThrowIfDisposed();
            if (!_active) return;

            var token = new object();
            currentToken = token;

            OnUpdate();
            if (!CheckToken(token)) return;

            foreach (Action action in onUpdate?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            hierarchy.Update();
            if (!CheckToken(token)) return;

            currentToken = null;
        }
        protected virtual void OnUpdate() { }


        public void Exit()
        {
            if (!_active) return;
            _active = false;

            var token = new object();
            currentToken = token;


            hierarchy.Exit();
            if (!CheckToken(token)) return;

            foreach (Action action in onExit?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }

            OnExit();
            if (!CheckToken(token)) return;

#if UNIENGINE
            stream.Exit();
#endif

            currentToken = null;
        }
        protected virtual void OnExit() { }

        private bool CheckToken(object token) => token == currentToken;

#if UNIENGINE
        public virtual Work SetStream(EventStream stream)
        {
            ThrowIfDisposed();

            this.stream.SetStream(stream);
            return this;
        }
        public virtual Work SetStream(Func<EventStream> streamFactory)
        {
            ThrowIfDisposed();

            stream.SetStream(streamFactory);
            return this;
        }
        public virtual Work RemoveStream()
        {
            ThrowIfDisposed();

            stream.RemoveStream();
            return this;
        }
#endif

        public void SetNext(object next, bool restartIfPossible = false)
        {
            ThrowIfDisposed();
            hierarchy.SetNext(GetName(next), restartIfPossible);
        }
        public void SetNext<T>(bool restartIfPossible = false)
        {
            ThrowIfDisposed();
            hierarchy.SetNext(GetName(typeof(T)), restartIfPossible);
        }
        public void SetNextWith(object next, params object[] args)
        {
            ThrowIfDisposed();
            hierarchy.SetNext(GetName(next), true, args);
        }
        public void SetNextWith<T>(params object[] args)
        {
            ThrowIfDisposed();
            hierarchy.SetNext(GetName(typeof(T)), true, args);
        }
        public void ClearNext()
        {
            ThrowIfDisposed();
            hierarchy.ClearNext();
        }

        public Work SetStartAction(Action action)
        {
            onEnter = action;
            return this;
        }
        public Work SetStartAction(Action<object[]> action)
        {
            onEnterWith = action;
            return this;
        }
        public Work SetUpdateAction(Action action)
        {
            onUpdate = action;
            return this;
        }
        public Work SetStopAction(Action action)
        {
            onExit = action;
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
            return hierarchy.AddChild(GetName(name), primary);
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
            return hierarchy.AddChild(work, primary);
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
            hierarchy.AddChild(work, primary);
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
            return hierarchy.RemoveChild(GetName(name));
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
            hierarchy.RemoveChild(GetName(name));
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
            hierarchy.SetPrimary(GetName(name));
            return this;
        }

        /// <summary>
        /// Attempts to retrieve the current child of the specified type.
        /// </summary>
        public bool TryGetCurrentChild<T>(out T current) where T : Work
        {
            current = null;

            if (hierarchy.CurrentChild is null)
                return false;
            if (hierarchy.CurrentChild is not T _current)
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


        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name, Ctx("The work has been disposed."));
        }

        public void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;

            Exit();

#if UNIENGINE
            stream.Dispose();
#endif
            hierarchy.Dispose();

            IsDisposed = true;
        }
        public virtual void OnDispose() { }


        private string Ctx(string message) => $"[Work '{Name}'] {message}";

        public override string ToString()
        {
            return $"{Name}, Active: {Active}\n" +
                $"Current: {hierarchy.CurrentChild?.Name ?? "None"}\n" +
                $"Primary: {(!string.IsNullOrWhiteSpace(hierarchy.PrimaryChild) ? hierarchy.PrimaryChild : "None")}\n" +
                $"Reserved: {(!string.IsNullOrWhiteSpace(hierarchy.ReservedChild) ? hierarchy.ReservedChild : "None")}";
        }
    }
}
