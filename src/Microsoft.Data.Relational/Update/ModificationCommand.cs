// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Relational.Update
{
    public class ModificationCommand
    {
        private readonly StateEntry _stateEntry;
        private readonly Table _table;
        private readonly KeyValuePair<Column, object>[] _columnValues;
        private readonly KeyValuePair<Column, object>[] _whereClauses;
        private readonly ModificationOperation _operation;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ModificationCommand()
        {
        }

        public ModificationCommand([NotNull] StateEntry stateEntry, [NotNull] Table table)
        {
            Check.NotNull(stateEntry, "stateEntry");
            
            if(!(stateEntry.EntityState.IsDirty()))
            {
                throw new NotSupportedException(Strings.FormatModificationFunctionInvalidEntityState(stateEntry.EntityState));
            }

            _stateEntry = stateEntry;
            _table = table;

            _operation =
                stateEntry.EntityState == EntityState.Added
                    ? ModificationOperation.Insert
                    : stateEntry.EntityState == EntityState.Modified
                        ? ModificationOperation.Update
                        : ModificationOperation.Delete;

            // TODO: this will need to be done lazily because the result can be different when 
            // the results are propagated to state entries for which commands have not been executed
            if (_operation == ModificationOperation.Insert)
            {
                _columnValues = GetColumnValues(table, stateEntry, true).ToArray();
            }
            else
            {
                if (_operation == ModificationOperation.Update)
                {
                    _columnValues = GetColumnValues(table, stateEntry, false).ToArray();
                }

                _whereClauses = GetWhereClauses(table, stateEntry).ToArray();
            }
        }

        public virtual ModificationOperation Operation
        {
            get { return _operation; }
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public virtual KeyValuePair<Column, object>[] ColumnValues
        {
            get { return _columnValues; }
        }

        public virtual KeyValuePair<Column, object>[] WhereClauses
        {
            get { return _whereClauses; }
        }

        private static IEnumerable<KeyValuePair<Column, object>> GetColumnValues(Table table, StateEntry stateEntry, bool includeKeys)
        {
            return table.Columns
                .Where(c =>
                    c.GenerationStrategy != StoreValueGenerationStrategy.Computed &&
                    c.GenerationStrategy != StoreValueGenerationStrategy.Identity &&
                    (includeKeys || !table.PrimaryKey.Columns.Contains(c)))
                .Select(c => new KeyValuePair<Column, object>(c, GetPropertyValue(stateEntry, c)));
                    
        }

        private static IEnumerable<KeyValuePair<Column, object>> GetWhereClauses(Table table, StateEntry stateEntry)
        {
            // TODO: Concurrency columns
            return
                table
                .PrimaryKey.Columns
                .Select(c => new KeyValuePair<Column, object>(c, GetPropertyValue(stateEntry, c)));
        }

        private static object GetPropertyValue(StateEntry stateEntry, Column column)
        {
            // TODO: poor man's model to store mapping
            return 
                stateEntry.GetPropertyValue(stateEntry.EntityType.Properties.Single(p => p.StorageName == column.Name));
        }
    }
}
