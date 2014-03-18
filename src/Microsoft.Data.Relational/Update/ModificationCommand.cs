// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Relational.Update
{
    internal class ModificationCommand
    {
        private readonly StateEntry _stateEntry;
        private readonly KeyValuePair<string, object>[] _columnValues;
        private readonly KeyValuePair<string, object>[] _whereClauses;
        private readonly ModificationOperation _operation;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ModificationCommand()
        {
        }

        public ModificationCommand([NotNull] StateEntry stateEntry)
        {
            Contract.Assert(
                stateEntry.EntityState == EntityState.Added || stateEntry.EntityState == EntityState.Modified ||
                stateEntry.EntityState == EntityState.Deleted, "Unexpected entity state");

            _stateEntry = stateEntry;
            _operation =
                stateEntry.EntityState == EntityState.Added
                    ? ModificationOperation.Insert
                    : stateEntry.EntityState == EntityState.Modified
                        ? ModificationOperation.Update
                        : ModificationOperation.Delete;

            if (_operation == ModificationOperation.Insert)
            {
                _columnValues = GetColumnValues(stateEntry, true).ToArray();
            }
            else
            {
                if (_operation == ModificationOperation.Update)
                {
                    _columnValues = GetColumnValues(stateEntry, false).ToArray();
                }

                _whereClauses = GetWhereClauses(stateEntry).ToArray();
            }
        }

        public virtual ModificationOperation Operation
        {
            get { return _operation; }
        }

        public virtual string TableName
        {
            get { return _stateEntry.EntityType.StorageName; }
        }

        public virtual KeyValuePair<string, object>[] ColumnValues
        {
            get { return _columnValues; }
        }

        public virtual KeyValuePair<string, object>[] WhereClauses
        {
            get { return _whereClauses; }
        }

        private static IEnumerable<KeyValuePair<string, object>> GetColumnValues(StateEntry stateEntry, bool includeKeys)
        {
            var entityType = stateEntry.EntityType;

            return entityType
                .Properties
                .Where(p =>
                    p.ValueGenerationStrategy != ValueGenerationStrategy.StoreComputed &&
                    p.ValueGenerationStrategy != ValueGenerationStrategy.StoreIdentity &&
                    (includeKeys || !entityType.GetKey().Properties.Contains(p)))
                .Select(p => new KeyValuePair<string, object>(p.StorageName, stateEntry.GetPropertyValue(p)));
        }

        private static IEnumerable<KeyValuePair<string, object>> GetWhereClauses(StateEntry stateEntry)
        {
            return
                stateEntry
                    .EntityType
                    .GetKey().Properties
                    .Select(k => new KeyValuePair<string, object>(k.Name, stateEntry.GetPropertyValue(k)));
        }
    }
}
