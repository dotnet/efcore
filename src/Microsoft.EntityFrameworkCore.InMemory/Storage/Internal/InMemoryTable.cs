// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class InMemoryTable<TKey> : IInMemoryTable
    {
        private readonly IPrincipalKeyValueFactory<TKey> _keyValueFactory;
        private readonly Dictionary<TKey, object[]> _rows;

        public InMemoryTable([NotNull] IPrincipalKeyValueFactory<TKey> keyValueFactory)
        {
            _keyValueFactory = keyValueFactory;
            _rows = new Dictionary<TKey, object[]>(keyValueFactory.EqualityComparer);
        }

        public virtual IReadOnlyList<object[]> SnapshotRows()
            => _rows.Values.ToList();

        public virtual void Create(IUpdateEntry entry)
            => _rows.Add(CreateKey(entry), CreateValueBuffer(entry));

        public virtual void Delete(IUpdateEntry entry)
        {
            var key = CreateKey(entry);

            if (_rows.ContainsKey(key))
            {
                _rows.Remove(key);
            }
            else
            {
                throw new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, new[] { entry });
            }
        }

        public virtual void Update(IUpdateEntry entry)
        {
            var key = CreateKey(entry);

            if (_rows.ContainsKey(key))
            {
                var properties = entry.EntityType.GetProperties().ToList();
                var valueBuffer = new object[properties.Count];

                for (var index = 0; index < valueBuffer.Length; index++)
                {
                    valueBuffer[index] = entry.IsModified(properties[index])
                        ? entry.GetCurrentValue(properties[index])
                        : _rows[key][index];
                }

                _rows[key] = valueBuffer;
            }
            else
            {
                throw new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, new[] { entry });
            }
        }

        private TKey CreateKey(IUpdateEntry entry)
            => _keyValueFactory.CreateFromCurrentValues((InternalEntityEntry)entry);

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(entry.GetCurrentValue).ToArray();
    }
}
