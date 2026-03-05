using System;
using System.Collections.Generic;
using static Infrastructure.StateMachines.Tools;

namespace Infrastructure.StateMachines.Fsm
{
    public partial class Work : IChildWork
    {
        // Front
        public string Name { get; init; }

        public bool Active
        {
            get => _active;
            set
            {
                if (value) Enter();
                else Exit();
            }
        }

        public Work Parent => hierarchy.Parent;

        public event Action Entered;
        public event Action<object[]> EnteredWith;
        public event Action Updated;
        public event Action Exited;

        public bool IsDisposed { get; private set; } = false;
        private bool isDisposing = false;

        // Property
        protected readonly HierarchyManager hierarchy;
#if UNIENGINE
        private readonly StreamManager stream;
#endif

        // Internal
        private bool _active = false;
        private object currentToken = null;


        // Content
        public Work(object name = null)
        {
            Name = GetName(name ?? GetType());
            hierarchy = new(this);
#if UNIENGINE
            stream = new(this);
#endif
        }

        public void Enter(params object[] args)
        {
            ThrowIfDisposed();

            if (!hierarchy.CanOpen)
                throw new InvalidOperationException(FormatLogMessage("Cannot enter because the parent is not active."));

            if (_active) return;
            _active = true;

            var token = new object();
            currentToken = token;

            OnEnter(args);
            if (!CheckToken(token)) return;

            foreach (Action action in Entered?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                action.Invoke();
                if (!CheckToken(token)) return;
            }
            foreach (Action<object[]> action in EnteredWith?.GetInvocationList() ?? Array.Empty<Delegate>())
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
        protected virtual void OnEnter(params object[] inputs) { }


        public void Update()
        {
            ThrowIfDisposed();
            if (!_active) return;

            var token = new object();
            currentToken = token;

            OnUpdate();
            if (!CheckToken(token)) return;

            foreach (Action action in Updated?.GetInvocationList() ?? Array.Empty<Delegate>())
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

            foreach (Action action in Exited?.GetInvocationList() ?? Array.Empty<Delegate>())
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
        public void SetNextToNone()
        {
            ThrowIfDisposed();
            hierarchy.SetNextToNone();
        }
        public void ClearNext()
        {
            ThrowIfDisposed();
            hierarchy.ClearNext();
        }

        public Work OnEntered(Action action)
        {
            Entered += action;
            return this;
        }
        public Work OnEntered<T>(Action<T> action) where T : Work
        {
            Entered += () => action(this as T);
            return this;
        }
        public Work OnEntered(Action<object[]> action)
        {
            EnteredWith += action;
            return this;
        }
        public Work OnUpdated(Action action)
        {
            Updated += action;
            return this;
        }
        public Work OnUpdated<T>(Action<T> action) where T : Work
        {
            Updated += () => action(this as T);
            return this;
        }
        public Work OnExited(Action action)
        {
            Exited += action;
            return this;
        }
        public Work OnExited<T>(Action<T> action) where T : Work
        {
            Exited += () => action(this as T);
            return this;
        }

        public Work SetActive(bool active)
        {
            Active = active;
            return this;
        }


        #region Child Management
        /// <summary>
        /// Attaches an existing <see cref="Work"/> object to this instance,
        /// and returns the attached child.
        /// </summary>
        /// <param name="work">The existing child instance to attach.</param>
        /// <param name="isPrimary">Indicates whether this child is considered primary.</param>
        /// <returns>The attached child of type <typeparamref name="T"/>.</returns>
        public Work AddChild<T>(T work, bool isPrimary = false) where T : Work
        {
            ThrowIfDisposed();
            hierarchy.AddChild(work.Name, work, isPrimary);
            return this;
        }

        /// <summary>
        /// Removes the child with the specified name from this instance,
        /// and returns the removed child <see cref="Work"/>.
        /// </summary>
        /// <param name="name">The name of the child to remove.</param>
        /// <returns>The removed child <see cref="Work"/>.</returns>
        public IChildWork RemoveChild(object name)
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

        public bool TryGetCurrentChild(out IChildWork current) => TryGetCurrentChild<IChildWork>(out current);
        /// <summary>
        /// Attempts to retrieve the current child of the specified type.
        /// </summary>
        public bool TryGetCurrentChild<T>(out T current) where T : IChildWork
        {
            current = default;

            if (hierarchy.CurrentChild is null)
                return false;
            if (hierarchy.CurrentChild is not T _current)
                return false;

            current = _current;
            return true;
        }

        public bool TryGetChild<T>(out T current) where T : IChildWork => TryGetChild(GetName(typeof(T)), out current);
        public bool TryGetChild<T>(string name, out T current) where T : IChildWork
        {
            current = default;

            if (!hierarchy.Children.ContainsKey(name))
                return false;
            if (hierarchy.Children[name] is not T _current)
                return false;

            current = _current;
            return true;
        }
        #endregion


        public string GetFullState()
        {
            var logs = new List<string> { Name };
            var current = this;

            while (current.TryGetCurrentChild(out IChildWork child))
            {
                logs.Add(current.Name);
                current = child as Work;

                if (current == null)
                    break;
            }

            return string.Join(" - ", logs);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name, FormatLogMessage("The work has been disposed."));
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


        protected string FormatLogMessage(string message) => $"[Work '{Name}'] {message}";

        public override string ToString()
        {
            return $"{Name}, Active: {Active}\n" +
                $"Current: {hierarchy.CurrentChild?.Name ?? "None"}\n" +
                $"Primary: {(!string.IsNullOrWhiteSpace(hierarchy.PrimaryChild) ? hierarchy.PrimaryChild : "None")}\n" +
                $"Reserved: {(!string.IsNullOrWhiteSpace(hierarchy.ReservedChild) ? hierarchy.ReservedChild : "None")}";
        }
    }
}
