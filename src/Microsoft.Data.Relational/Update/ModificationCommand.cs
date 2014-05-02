// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Utilities;

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

            if (!stateEntry.EntityState.IsDirty())
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
                _columnValues = GetColumnValues(table, true).ToArray();
            }
            else
            {
                if (_operation == ModificationOperation.Update)
                {
                    _columnValues = GetColumnValues(table, false).ToArray();
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

        public virtual StateEntry StateEntry
        {
            get { return _stateEntry; }
        }

        internal virtual bool RequiresResultPropagation
        {
            get
            {
                if (Operation != ModificationOperation.Delete)
                {
                    var storeGeneratedColumns = _table.GetStoreGeneratedColumns();

                    return (Operation == ModificationOperation.Update
                        ? storeGeneratedColumns.Except(_table.PrimaryKey.Columns)
                        : storeGeneratedColumns).Any();
                }

                return false;
            }
        }

        private IEnumerable<KeyValuePair<Column, object>> GetColumnValues(Table table, bool includeKeyColumns)
        {
            var nonStoreGeneratedColumns = table.Columns.Except(table.GetStoreGeneratedColumns());

            return
                (includeKeyColumns ? nonStoreGeneratedColumns : nonStoreGeneratedColumns.Except(table.PrimaryKey.Columns))
                    .Select(c => new KeyValuePair<Column, object>(c, GetPropertyValue(_stateEntry, c)));
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
            return stateEntry[GetProperty(stateEntry, column.Name)];
        }

        private static IProperty GetProperty(StateEntry stateEntry, string columnName)
        {
            // TODO: poor man's model to store mapping
            return stateEntry.EntityType.Properties.Single(p => p.StorageName == columnName);
        }
    }
}
