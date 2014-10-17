// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDatabase : IEnumerable<InMemoryDatabase.InMemoryTable>
    {
        private readonly ILogger _logger;

        private readonly ThreadSafeLazyRef<ImmutableDictionary<IEntityType, InMemoryTable>> _tables
            = new ThreadSafeLazyRef<ImmutableDictionary<IEntityType, InMemoryTable>>(
                () => ImmutableDictionary<IEntityType, InMemoryTable>.Empty);

        public InMemoryDatabase([CanBeNull] IEnumerable<ILoggerFactory> loggerFactories)
        {
            var factory = (loggerFactories == null ? null : loggerFactories.FirstOrDefault()) ?? new LoggerFactory();

            _logger = factory.Create<InMemoryDatabase>();
        }

        /// <summary>
        /// Returns true just after the database has been created, false thereafter
        /// </summary>
        /// <returns>
        ///     true if the database has just been created, false otherwise
        /// </returns>
        public virtual bool IsCreated([NotNull] IModel model)
        {
            var returnValue = !_tables.HasValue;
            var _ = _tables.Value;
            return returnValue;
        }

        public virtual void Clear()
        {
            _tables.ExchangeValue(ts => ImmutableDictionary<IEntityType, InMemoryTable>.Empty);
        }

        public virtual InMemoryTable GetTable([NotNull] IEntityType entityType)
        {
            InMemoryTable table;

            return _tables.HasValue
                   && _tables.Value.TryGetValue(entityType, out table)
                ? table
                : InMemoryTable.Empty;
        }

        public virtual int ExecuteTransaction([NotNull] IEnumerable<StateEntry> stateEntries)
        {
            var rowsAffected = 0;

            _tables.ExchangeValue(ts =>
                {
                    rowsAffected = 0;

                    foreach (var stateEntry in stateEntries)
                    {
                        InMemoryTable table;
                        if (!ts.TryGetValue(stateEntry.EntityType, out table))
                        {
                            ts = ts.Add(stateEntry.EntityType, table = new InMemoryTable());
                        }

                        switch (stateEntry.EntityState)
                        {
                            case EntityState.Added:
                                table.Create(stateEntry);
                                break;
                            case EntityState.Deleted:
                                table.Delete(stateEntry);
                                break;
                            case EntityState.Modified:
                                table.Update(stateEntry);
                                break;
                        }

                        rowsAffected++;
                    }

                    return ts;
                });

            _logger.WriteInformation(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Saved {0} entities to in-memory database.",
                    rowsAffected));

            return rowsAffected;
        }

        public virtual IEnumerator<InMemoryTable> GetEnumerator()
        {
            return _tables.HasValue
                ? _tables.Value.Values.GetEnumerator()
                : Enumerable.Empty<InMemoryTable>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class InMemoryTable : IEnumerable<object[]>
        {
            internal static readonly InMemoryTable Empty = new InMemoryTable();

            private readonly ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>> _rows
                = new ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>>(
                    () => ImmutableDictionary<EntityKey, object[]>.Empty);

            internal void Create(StateEntry stateEntry)
            {
                _rows.ExchangeValue(rs => rs.Add(stateEntry.GetPrimaryKeyValue(), stateEntry.GetValueBuffer()));
            }

            internal void Delete(StateEntry stateEntry)
            {
                _rows.ExchangeValue(rs => rs.Remove(stateEntry.GetPrimaryKeyValue()));
            }

            internal void Update(StateEntry stateEntry)
            {
                _rows.ExchangeValue(rs => rs.SetItem(stateEntry.GetPrimaryKeyValue(), stateEntry.GetValueBuffer()));
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
