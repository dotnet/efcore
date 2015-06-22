// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.InMemory.Metadata;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryStore : IInMemoryStore
    {
        private readonly ILogger _logger;

        private readonly ThreadSafeLazyRef<ImmutableDictionary<IEntityType, InMemoryTable>> _tables
            = new ThreadSafeLazyRef<ImmutableDictionary<IEntityType, InMemoryTable>>(
                () => ImmutableDictionary<IEntityType, InMemoryTable>.Empty.WithComparers(new EntityTypeNameEqualityComparer()));

        public InMemoryStore([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<InMemoryStore>();
        }

        /// <summary>
        ///     Returns true just after the database has been created, false thereafter
        /// </summary>
        /// <returns>
        ///     true if the database has just been created, false otherwise
        /// </returns>
        public virtual bool EnsureCreated(IModel model)
        {
            Check.NotNull(model, nameof(model));

            var returnValue = !_tables.HasValue;

            // ReSharper disable once UnusedVariable
            var _ = _tables.Value;

            return returnValue;
        }

        public virtual void Clear()
            => _tables.ExchangeValue(ts => ImmutableDictionary<IEntityType, InMemoryTable>.Empty);

        public virtual IEnumerable<InMemoryTable> GetTables(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (!_tables.HasValue)
            {
                yield break;
            }

            foreach (var et in entityType.GetConcreteTypesInHierarchy())
            {
                InMemoryTable table;

                if (_tables.Value.TryGetValue(et, out table))
                {
                    yield return table;
                }
            }
        }

        public virtual int ExecuteTransaction(IEnumerable<InternalEntityEntry> entries)
        {
            Check.NotNull(entries, nameof(entries));

            var rowsAffected = 0;

            _tables.ExchangeValue(ts =>
                {
                    rowsAffected = 0;

                    foreach (var entry in entries)
                    {
                        var entityType = entry.EntityType;

                        Debug.Assert(!entityType.IsAbstract);

                        InMemoryTable table;
                        if (!ts.TryGetValue(entityType, out table))
                        {
                            ts = ts.Add(entityType, table = new InMemoryTable(entityType));
                        }

                        switch (entry.EntityState)
                        {
                            case EntityState.Added:
                                table.Create(entry);
                                break;
                            case EntityState.Deleted:
                                table.Delete(entry);
                                break;
                            case EntityState.Modified:
                                table.Update(entry);
                                break;
                        }

                        rowsAffected++;
                    }

                    return ts;
                });

            _logger.LogInformation(rowsAffected, ra => Strings.LogSavedChanges(ra));

            return rowsAffected;
        }

        public virtual IEnumerator<InMemoryTable> GetEnumerator()
            => _tables.HasValue
                ? _tables.Value.Values.GetEnumerator()
                : Enumerable.Empty<InMemoryTable>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class InMemoryTable : IEnumerable<object[]>
        {
            private readonly ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>> _rows
                = new ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>>(
                    () => ImmutableDictionary<EntityKey, object[]>.Empty);

            public InMemoryTable([NotNull] IEntityType entityType)
            {
                Check.NotNull(entityType, nameof(entityType));

                EntityType = entityType;
            }

            public virtual IEntityType EntityType { get; private set; }

            internal void Create(InternalEntityEntry entry)
            {
                _rows.ExchangeValue(rs => rs.Add(entry.GetPrimaryKeyValue(), entry.GetValueBuffer()));
            }

            internal void Delete(InternalEntityEntry entry)
            {
                _rows.ExchangeValue(rs => rs.Remove(entry.GetPrimaryKeyValue()));
            }

            internal void Update(InternalEntityEntry entry)
            {
                _rows.ExchangeValue(rs => rs.SetItem(entry.GetPrimaryKeyValue(), entry.GetValueBuffer()));
            }

            public virtual IEnumerator<object[]> GetEnumerator()
            {
                return _rows.HasValue
                    ? _rows.Value.Values.GetEnumerator()
                    : Enumerable.Empty<object[]>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
