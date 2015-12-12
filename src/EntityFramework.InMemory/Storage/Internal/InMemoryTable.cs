// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class InMemoryTable<TKey> : IInMemoryTable
    {
        private readonly IPrincipalKeyValueFactory<TKey> _keyValueFactory;

        private readonly Dictionary<TKey, object[]> _rows = new Dictionary<TKey, object[]>();

        public InMemoryTable([NotNull] IPrincipalKeyValueFactory<TKey> keyValueFactory)
        {
            _keyValueFactory = keyValueFactory;
        }

        public virtual IReadOnlyList<object[]> SnapshotRows()
            => _rows.Values.ToList();

        public virtual void Create(IUpdateEntry entry)
            => _rows.Add(CreateKey(entry), CreateValueBuffer(entry));

        public virtual void Delete(IUpdateEntry entry)
            => _rows.Remove(CreateKey(entry));

        public virtual void Update(IUpdateEntry entry)
            => _rows[CreateKey(entry)] = CreateValueBuffer(entry);

        private TKey CreateKey(IUpdateEntry entry)
            => _keyValueFactory.CreateFromCurrentValues((InternalEntityEntry)entry);

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(p => entry.GetValue(p)).ToArray();
    }
}
