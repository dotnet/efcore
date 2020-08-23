// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents a conceptual command to the database to insert/update/delete a row.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public class ModificationCommand
    {
        private readonly Func<string> _generateParameterName;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly IComparer<IUpdateEntry> _comparer;
        private readonly List<IUpdateEntry> _entries = new List<IUpdateEntry>();
        private IReadOnlyList<ColumnModification> _columnModifications;
        private bool _requiresResultPropagation;
        private bool _mainEntryAdded;

        /// <summary>
        ///     Initializes a new <see cref="ModificationCommand" /> instance.
        /// </summary>
        /// <param name="name"> The name of the table containing the data to be modified. </param>
        /// <param name="schema"> The schema containing the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="generateParameterName"> A delegate to generate parameter names. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="comparer"> A <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />s. </param>
        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            [CanBeNull] IComparer<IUpdateEntry> comparer)
            : this(
                Check.NotEmpty(name, nameof(name)),
                schema,
                null,
                sensitiveLoggingEnabled)
        {
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            _generateParameterName = generateParameterName;
            _comparer = comparer;
        }

        /// <summary>
        ///     Initializes a new <see cref="ModificationCommand" /> instance.
        /// </summary>
        /// <param name="name"> The name of the table containing the data to be modified. </param>
        /// <param name="schema"> The schema containing the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="columnModifications"> The list of <see cref="ColumnModification" />s needed to perform the insert, update, or delete. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schema,
            [CanBeNull] IReadOnlyList<ColumnModification> columnModifications,
            bool sensitiveLoggingEnabled)
        {
            Check.NotNull(name, nameof(name));

            TableName = name;
            Schema = schema;
            _columnModifications = columnModifications;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
        }

        /// <summary>
        ///     The name of the table containing the data to be modified.
        /// </summary>
        public virtual string TableName { get; }

        /// <summary>
        ///     The schema containing the table, or <see langword="null" /> to use the default schema.
        /// </summary>
        public virtual string Schema { get; }

        /// <summary>
        ///     The <see cref="IUpdateEntry" />s that represent the entities that are mapped to the row
        ///     to update.
        /// </summary>
        public virtual IReadOnlyList<IUpdateEntry> Entries
            => _entries;

        /// <summary>
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="EntityFrameworkCore.EntityState.Deleted" />).
        /// </summary>
        public virtual EntityState EntityState
        {
            get
            {
                if (_mainEntryAdded)
                {
                    var mainEntry = _entries[0];
                    if (mainEntry.SharedIdentityEntry == null)
                    {
                        return mainEntry.EntityState;
                    }

                    return mainEntry.SharedIdentityEntry.EntityType == mainEntry.EntityType
                        || mainEntry.SharedIdentityEntry.EntityType.GetTableMappings()
                            .Any(m => m.Table.Name == TableName && m.Table.Schema == Schema)
                            ? EntityState.Modified
                            : mainEntry.EntityState;
                }

                return EntityState.Modified;
            }
        }

        /// <summary>
        ///     The list of <see cref="ColumnModification" />s needed to perform the insert, update, or delete.
        /// </summary>
        public virtual IReadOnlyList<ColumnModification> ColumnModifications
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _columnModifications, this, command => command.GenerateColumnModifications());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Conditional("DEBUG")]
        [EntityFrameworkInternal]
        public virtual void AssertColumnsNotInitialized()
        {
            if (_columnModifications != null)
            {
                throw new Exception("_columnModifications have been initialized prematurely");
            }
        }

        /// <summary>
        ///     Indicates whether or not the database will return values for some mapped properties
        ///     that will then need to be propagated back to the tracked entities.
        /// </summary>
        public virtual bool RequiresResultPropagation
        {
            get
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = ColumnModifications;

                return _requiresResultPropagation;
            }
        }

        /// <summary>
        ///     Adds an <see cref="IUpdateEntry" /> to this command representing an entity to be inserted, updated, or deleted.
        /// </summary>
        /// <param name="entry"> The entry representing the entity to add. </param>
        /// <param name="mainEntry"> A value indicating whether this is the main entry for the row. </param>
        public virtual void AddEntry([NotNull] IUpdateEntry entry, bool mainEntry)
        {
            Check.NotNull(entry, nameof(entry));

            switch (entry.EntityState)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    throw new ArgumentException(RelationalStrings.ModificationCommandInvalidEntityState(entry.EntityState));
            }

            if (mainEntry)
            {
                Check.DebugAssert(!_mainEntryAdded, "Only expected a single main entry");

                for (var i = 0; i < _entries.Count; i++)
                {
                    ValidateState(entry, _entries[i]);
                }

                _mainEntryAdded = true;
                _entries.Insert(0, entry);
            }
            else
            {
                if (_mainEntryAdded)
                {
                    ValidateState(_entries[0], entry);
                }

                _entries.Add(entry);
            }

            _columnModifications = null;
        }

        private void ValidateState(IUpdateEntry mainEntry, IUpdateEntry entry)
        {
            var mainEntryState = mainEntry.SharedIdentityEntry == null
                ? mainEntry.EntityState
                : EntityState.Modified;
            if (mainEntryState == EntityState.Modified)
            {
                return;
            }

            var entryState = entry.SharedIdentityEntry == null
                ? entry.EntityState
                : EntityState.Modified;
            if (mainEntryState != entryState)
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowUpdateTypesSensitive(
                            entry.EntityType.DisplayName(),
                            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                            entryState,
                            mainEntry.EntityType.DisplayName(),
                            mainEntry.BuildCurrentValuesString(mainEntry.EntityType.FindPrimaryKey().Properties),
                            mainEntryState));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowUpdateTypes(
                        entry.EntityType.DisplayName(),
                        entryState,
                        mainEntry.EntityType.DisplayName(),
                        mainEntryState));
            }
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var state = EntityState;
            var adding = state == EntityState.Added;
            var updating = state == EntityState.Modified;
            var columnModifications = new List<ColumnModification>();
            Dictionary<string, ColumnValuePropagator> sharedTableColumnMap = null;

            if (_entries.Count > 1
                || (_entries.Count == 1 && _entries[0].SharedIdentityEntry != null))
            {
                sharedTableColumnMap = new Dictionary<string, ColumnValuePropagator>();

                if (_comparer != null)
                {
                    _entries.Sort(_comparer);
                }

                foreach (var entry in _entries)
                {
                    var tableMapping = GetTableMapping(entry.EntityType);
                    if (tableMapping == null)
                    {
                        continue;
                    }

                    if (entry.SharedIdentityEntry != null)
                    {
                        var sharedTableMapping = GetTableMapping(entry.SharedIdentityEntry.EntityType);
                        if (sharedTableMapping != null)
                        {
                            InitializeSharedColumns(entry.SharedIdentityEntry, sharedTableMapping, updating, sharedTableColumnMap);
                        }
                    }

                    InitializeSharedColumns(entry, tableMapping, updating, sharedTableColumnMap);
                }
            }

            foreach (var entry in _entries)
            {
                var nonMainEntry = updating
                    && (entry.EntityState == EntityState.Deleted
                        || entry.EntityState == EntityState.Added);

                var tableMapping = GetTableMapping(entry.EntityType);
                if (tableMapping == null)
                {
                    continue;
                }

                foreach (var columnMapping in tableMapping.ColumnMappings)
                {
                    var property = columnMapping.Property;
                    var column = (IColumn)columnMapping.Column;
                    var isKey = property.IsPrimaryKey();
                    var isCondition = !adding && (isKey || property.IsConcurrencyToken);
                    var readValue = state != EntityState.Deleted && entry.IsStoreGenerated(property);

                    ColumnValuePropagator columnPropagator = null;
                    sharedTableColumnMap?.TryGetValue(column.Name, out columnPropagator);

                    var writeValue = false;
                    if (!readValue)
                    {
                        if (adding)
                        {
                            writeValue = property.GetBeforeSaveBehavior() == PropertySaveBehavior.Save;
                        }
                        else if ((updating && property.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
                            || (!isKey && nonMainEntry))
                        {
                            writeValue = columnPropagator?.TryPropagate(property, entry)
                                ?? entry.IsModified(property);
                        }
                    }

                    if (readValue
                        || writeValue
                        || isCondition)
                    {
                        if (readValue)
                        {
                            _requiresResultPropagation = true;
                        }

                        var columnModification = new ColumnModification(
                            entry,
                            property,
                            column,
                            _generateParameterName,
                            columnMapping.TypeMapping,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            _sensitiveLoggingEnabled);

                        if (columnPropagator != null
                            && column.PropertyMappings.Count() != 1)
                        {
                            if (columnPropagator.ColumnModification != null)
                            {
                                columnPropagator.ColumnModification.AddSharedColumnModification(columnModification);

                                continue;
                            }

                            columnPropagator.ColumnModification = columnModification;
                        }

                        columnModifications.Add(columnModification);
                    }
                }
            }

            return columnModifications;
        }

        private ITableMappingBase GetTableMapping(IEntityType entityType)
        {
            ITableMappingBase tableMapping = null;
            foreach (var mapping in entityType.GetTableMappings())
            {
                var table = ((ITableMappingBase)mapping).Table;
                if (table.Name == TableName
                    && table.Schema == Schema)
                {
                    tableMapping = mapping;
                    break;
                }
            }

            return tableMapping;
        }

        private void InitializeSharedColumns(
            IUpdateEntry entry,
            ITableMappingBase tableMapping,
            bool updating,
            Dictionary<string, ColumnValuePropagator> columnMap)
        {
            foreach (var columnMapping in tableMapping.ColumnMappings)
            {
                var columnName = columnMapping.Column.Name;
                if (!columnMap.TryGetValue(columnName, out var columnPropagator))
                {
                    columnPropagator = new ColumnValuePropagator();
                    columnMap.Add(columnName, columnPropagator);
                }

                if (updating)
                {
                    columnPropagator.RecordValue(columnMapping.Property, entry);
                }
            }
        }

        /// <summary>
        ///     Reads values returned from the database in the given <see cref="ValueBuffer" /> and
        ///     propagates them back to into the appropriate <see cref="ColumnModification" />
        ///     from which the values can be propagated on to tracked entities.
        /// </summary>
        /// <param name="valueBuffer"> The buffer containing the values read from the database. </param>
        public virtual void PropagateResults(ValueBuffer valueBuffer)
        {
            Check.NotNull(valueBuffer, nameof(valueBuffer));

            // Note that this call sets the value into a sidecar and will only commit to the actual entity
            // if SaveChanges is successful.
            var index = 0;
            foreach (var modification in ColumnModifications.Where(o => o.IsRead))
            {
                modification.Value = valueBuffer[index++];
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = $"{EntityState}: {TableName}";
            if (_columnModifications == null)
            {
                return result;
            }

            result += "(" + string.Join(", ", _columnModifications.Where(m => m.IsKey).Select(m => m.OriginalValue.ToString())) + ")";
            return result;
        }

        private sealed class ColumnValuePropagator
        {
            private bool _write;
            private object _originalValue;
            private object _currentValue;

            public ColumnModification ColumnModification { get; set; }

            public void RecordValue(IProperty property, IUpdateEntry entry)
            {
                switch (entry.EntityState)
                {
                    case EntityState.Modified:
                        if (!_write
                            && entry.IsModified(property))
                        {
                            _write = true;
                            _currentValue = entry.GetCurrentValue(property);
                        }

                        break;
                    case EntityState.Added:
                        _currentValue = entry.GetCurrentValue(property);

                        var comparer = property.GetValueComparer();
                        if (comparer == null)
                        {
                            _write = !Equals(_originalValue, _currentValue);
                        }
                        else
                        {
                            _write = !comparer.Equals(_originalValue, _currentValue);
                        }

                        break;
                    case EntityState.Deleted:
                        _originalValue = entry.GetOriginalValue(property);
                        if (!_write
                            && !property.IsPrimaryKey())
                        {
                            _write = true;
                            _currentValue = null;
                        }

                        break;
                }
            }

            public bool TryPropagate(IProperty property, IUpdateEntry entry)
            {
                if (_write
                    && (entry.EntityState == EntityState.Unchanged
                        || (entry.EntityState == EntityState.Modified && !entry.IsModified(property))
                        || (entry.EntityState == EntityState.Added && Equals(_originalValue, entry.GetCurrentValue(property)))))
                {
                    entry.SetStoreGeneratedValue(property, _currentValue);

                    return false;
                }

                return _write;
            }
        }
    }
}
