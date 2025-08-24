using System;
using System.Collections.Generic;
using System.Linq;

namespace UniEngine.StateMachines.BT
{
    public readonly struct SelectionRequest
    {
        public Func<IBTNode, bool> Predicate { get; }
        public bool ThrowIfNotFound { get; }

        public object[] Inputs { get; }

        public EntryPolicy EntryPolicy { get; }
        public RerunPolicy RerunPolicy { get; }


        public SelectionRequest(
            bool pass,
            object[] inputs = null,
            EntryPolicy entryPolicy = EntryPolicy.CheckOnOpen,
            RerunPolicy rerunPolicy = RerunPolicy.IgnoreIfRunning,
            bool throwIfNotFound = true)
            : this(
            predicate: _ => pass,
            inputs: inputs,
            entryPolicy: entryPolicy,
            rerunPolicy: rerunPolicy,
            throwIfNotFound: throwIfNotFound){ }


        public SelectionRequest(
            string name,
            object[] inputs = null,
            EntryPolicy entryPolicy = EntryPolicy.CheckOnOpen,
            RerunPolicy rerunPolicy = RerunPolicy.IgnoreIfRunning,
            bool throwIfNotFound = true)
            : this(
            predicate: n => n.Name == name,
            inputs: inputs,
            entryPolicy: entryPolicy,
            rerunPolicy: rerunPolicy,
            throwIfNotFound : throwIfNotFound){ }


        public SelectionRequest(
            Func<IBTNode, bool> predicate,
            object[] inputs = null,
            EntryPolicy entryPolicy = EntryPolicy.CheckOnOpen,
            RerunPolicy rerunPolicy = RerunPolicy.IgnoreIfRunning,
            bool throwIfNotFound = true)
        {
            Predicate = predicate;
            Inputs = inputs;

            EntryPolicy = entryPolicy;
            RerunPolicy = rerunPolicy;

            ThrowIfNotFound = throwIfNotFound;
        }


        public static void ThrowIfNullOrEmpty(IEnumerable<SelectionRequest> requests, Func<string, string> ctx = null)
        {
            ctx ??= m => m;

            if (requests == null)
                throw new ArgumentNullException(nameof(requests), ctx("Requests cannot be null."));
            if (!requests.Any())
                throw new ArgumentException(ctx("At least one request must exist."), nameof(requests));
        }
    }
}
