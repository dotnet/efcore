// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal sealed class IDictionaryDebugView<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        private readonly IDictionary<TKey, TValue> _dict = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items
        {
            get
            {
                var items = new KeyValuePair<TKey, TValue>[_dict.Count];
                _dict.CopyTo(items, 0);
                return items;
            }
        }
    }

    internal sealed class DictionaryKeyCollectionDebugView<TKey, TValue>(ICollection<TKey> collection)
    {
        private readonly ICollection<TKey> _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                var items = new TKey[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    internal sealed class DictionaryValueCollectionDebugView<TKey, TValue>(ICollection<TValue> collection)
    {
        private readonly ICollection<TValue> _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                var items = new TValue[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
