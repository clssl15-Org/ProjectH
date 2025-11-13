using System;
using System.Collections;
using System.Collections.Generic;

namespace Infrastructure
{
    public class AdaptiveDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary
    {
        // Front
        public int Count => isIterating
            ? items.Count - removingKeys.Count + pendingItems.Count
            : items.Count;

        public bool IsReadOnly { get; } = false;
        public bool IsFixedSize { get; } = false;
        public bool DeferredUpdate { get; private set; }

        public ICollection<TKey> Keys => !isIterating ? items.Keys : throw new NotSupportedException("The Keys property cannot be accessed during iteration.");
        public ICollection<TValue> Values => !isIterating ? items.Values : throw new NotSupportedException("The Values property cannot be accessed during iteration.");

        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new();

        /// <summary>
        /// Gets or sets the value for the specified key
        /// </summary>
        /// <remarks>
        /// - If modified during a deferred iteration, the value will not be included in the current iteration but will be available in the next iteration.<br/>
        /// - If modified during an immediate iteration, the value will be traversed at the end of the current iteration, regardless of whether it has already been iterated over.
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                    throw new KeyNotFoundException($"The given key '{key}' was not present in the AdaptiveDictionary.");

                return value;
            }
            set
            {
                if (isIterating)
                {
                    Remove(key);
                    Add(key, value);
                }
                else
                    items[key] = value;
            }
        }

        // Internal
        readonly Dictionary<TKey, TValue> items = new();

        readonly Dictionary<TKey, TValue> pendingItems = new();
        readonly HashSet<TKey> removingKeys = new();

        bool isIterating = false;


        // Content
        public AdaptiveDictionary() : this(true) { }
        public AdaptiveDictionary(bool deferredUpdate)
        {
            DeferredUpdate = deferredUpdate;
        }

        #region Iteration
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (isIterating) throw new InvalidOperationException("AdaptiveDictionary is already being iterated. Nested iterations are not allowed.");
            isIterating = true;

            try
            {
                Dictionary<TKey, TValue> items = this.items;

                while (isIterating)
                {
                    using var enumerator = items.GetEnumerator();

                    while (isIterating && enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (removingKeys.Contains(item.Key))
                            continue;

                        yield return item;
                    }

                    if (!isIterating || DeferredUpdate || pendingItems.Count == 0)
                        break;

                    items = new Dictionary<TKey, TValue>(pendingItems);
                    ApplyChanges();
                }
            }
            finally
            {
                ApplyChanges();
                isIterating = false;
            }


            void ApplyChanges()
            {
                foreach (var key in removingKeys)
                    items.Remove(key);

                foreach (var item in pendingItems)
                    items[item.Key] = item.Value;

                removingKeys.Clear();
                pendingItems.Clear();
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public void ForEach(Action<KeyValuePair<TKey, TValue>> selector)
        {
            foreach (var item in this)
                selector(item);
        }
        public void ForEach(Action<TKey, TValue> selector)
        {
            foreach (var (key, value) in this)
                selector(key, value);
        }
        public void ForEach(Action<TValue> selector)
        {
            foreach (var (_, value) in this)
                selector(value);
        }
        #endregion


        public bool ContainsKey(TKey key) => TryGetValue(key, out _);
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (isIterating)
            {
                if (pendingItems.TryGetValue(key, out value))
                    return true;

                if (items.TryGetValue(key, out value))
                    return !removingKeys.Contains(key);

                return false;
            }
            else
                return items.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Add(TKey key, TValue value)
        {
            if (isIterating)
            {
                if (ContainsKey(key))
                    throw new ArgumentException($"An item with the same key '{key}' already exists.");

                pendingItems[key] = value;
            }
            else
                items.Add(key, value);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);
        public bool Remove(TKey key)
        {
            if (isIterating)
            {
                if (pendingItems.ContainsKey(key))
                {
                    pendingItems.Remove(key);
                    return true;
                }

                if (removingKeys.Contains(key))
                    return false;

                if (items.ContainsKey(key))
                {
                    removingKeys.Add(key);
                    return true;
                }

                return false;
            }
            else
                return items.Remove(key);
        }

        public void Clear()
        {
            items.Clear();
            removingKeys.Clear();
            pendingItems.Clear();

            isIterating = false;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!TryGetValue(item.Key, out var value)) return false;
            return item.Value.Equals(value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (isIterating)
                throw new InvalidOperationException("AdaptiveDictionary cannot be copied during iteration.");
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Target array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be non-negative.");
            if (array.Length - arrayIndex < items.Count)
                throw new ArgumentException("The target array is not large enough to hold the collection items.");

            foreach (var kvp in items)
                array[arrayIndex++] = kvp;
        }


        #region IDictionary
        ICollection IDictionary.Keys => !isIterating ? items.Keys : throw new NotSupportedException("The Count property cannot be accessed during iteration.");
        ICollection IDictionary.Values => !isIterating ? items.Values : throw new NotSupportedException("The Value property cannot be accessed during iteration.");

        object IDictionary.this[object key]
        {
            get
            {
                if (key is not TKey tKey)
                    throw new ArgumentException("Key is of incorrect type.", nameof(key));

                return this[tKey];
            }
            set
            {
                if (key is not TKey tKey)
                    throw new ArgumentException("Key is of incorrect type.", nameof(key));
                if (value is not TValue tValue)
                    throw new ArgumentException("Value is of incorrect type.", nameof(value));

                this[tKey] = tValue;
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator<TKey, TValue>(this);
        class DictionaryEnumerator<UKey, UValue> : IDictionaryEnumerator, IDisposable
        {
            IEnumerator<KeyValuePair<UKey, UValue>> _currentEnumerator;
            readonly AdaptiveDictionary<UKey, UValue> _dictionary;

            public DictionaryEnumerator(AdaptiveDictionary<UKey, UValue> dictionary)
            {
                _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
                _currentEnumerator = _dictionary.GetEnumerator();
            }

            public object Key => _currentEnumerator.Current.Key;
            public object Value => _currentEnumerator.Current.Value;

            public DictionaryEntry Entry => new(Key, Value);

            public bool MoveNext() => _currentEnumerator.MoveNext();
            public void Reset() => throw new NotSupportedException("AdaptiveDictionary cannot be reset during iteration.");

            public object Current => Entry;

            public void Dispose()
            {
                _currentEnumerator?.Dispose();
                _currentEnumerator = null;
            }
        }


        void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);
        void IDictionary.Remove(object key) => Remove((TKey)key);
        bool IDictionary.Contains(object key) => ContainsKey((TKey)key);

        void ICollection.CopyTo(Array array, int index)
        {
            if (isIterating)
                throw new NotSupportedException("AdaptiveDictionary cannot be copied during iteration.");
            if (array == null)
                if (array == null)
                    throw new ArgumentNullException(nameof(array), "Target array cannot be null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Array index must be non-negative.");
            if (array.Rank != 1)
                throw new ArgumentException("Target array must be single-dimensional.", nameof(array));
            if (array.Length - index < items.Count)
                throw new ArgumentException("The target array is not large enough to hold the collection items.");

            if (array is KeyValuePair<TKey, TValue>[] pairs)
            {
                CopyTo(pairs, index);
            }
            else
            {
                foreach (var item in items)
                {
                    array.SetValue(item, index++);
                }
            }
        }
        #endregion
    }
}
