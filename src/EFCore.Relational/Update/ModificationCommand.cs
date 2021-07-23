// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class ModificationCommand : IMutableModificationCommand
    {
        private readonly Func<string>? _generateParameterName;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly IComparer<IUpdateEntry>? _comparer;
        private readonly List<IUpdateEntry> _entries = new();
        private List<IColumnModification>? _columnModifications;
        private bool _requiresResultPropagation;
        private bool _mainEntryAdded;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Update>? _logger;

        /// <summary>
        ///     Initializes a new <see cref="ModificationCommand" /> instance.
        /// </summary>
        /// <param name="modificationCommandParameters"> Creation parameters. </param>
        public ModificationCommand(ModificationCommandParameters modificationCommandParameters)
        {
            TableName = modificationCommandParameters.TableName;
            Schema = modificationCommandParameters.Schema;
            _generateParameterName = modificationCommandParameters.GenerateParameterName;
            _sensitiveLoggingEnabled = modificationCommandParameters.SensitiveLoggingEnabled;
            _comparer = modificationCommandParameters.Comparer;
            _logger = modificationCommandParameters.Logger;
        }

        /// <summary>
        ///     The name of the table containing the data to be modified.
        /// </summary>
        public virtual string TableName { get; }

        /// <summary>
        ///     The schema containing the table, or <see langword="null" /> to use the default schema.
        /// </summary>
        public virtual string? Schema { get; }

        /// <summary>
        ///     The <see cref="IUpdateEntry" />s that represent the entities that are mapped to the row
        ///     to update.
        /// </summary>
        public virtual IReadOnlyList<IUpdateEntry> Entries
            => _entries;

        /// <summary>
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
        /// </summary>
        public virtual EntityState EntityState { get; private set; } = EntityState.Modified;

        /// <summary>
        ///     The list of <see cref="IColumnModification" /> needed to perform the insert, update, or delete.
        /// </summary>
        public virtual IReadOnlyList<IColumnModification> ColumnModifications
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _columnModifications, this, static command => command.GenerateColumnModifications());

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
        ///     Indicates whether the database will return values for some mapped properties
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddEntry(IUpdateEntry entry, bool mainEntry)
        {
            Check.NotNull(entry, nameof(entry));

            AssertColumnsNotInitialized();

            switch (entry.EntityState)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ModificationCommandInvalidEntityStateSensitive(
                                entry.EntityType.DisplayName(),
                                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                                entry.EntityState));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ModificationCommandInvalidEntityState(
                            entry.EntityType.DisplayName(),
                            entry.EntityState));
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

                EntityState = entry.SharedIdentityEntry == null
                    ? entry.EntityState
                    : entry.SharedIdentityEntry.EntityType == entry.EntityType
                        || entry.SharedIdentityEntry.EntityType.GetTableMappings()
                        .Any(m => m.Table.Name == TableName && m.Table.Schema == Schema)
                        ? EntityState.Modified
                        : entry.EntityState;
            }
            else
            {
                if (_mainEntryAdded)
                {
                    ValidateState(_entries[0], entry);
                }

                _entries.Add(entry);
            }
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
                            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                            entryState,
                            mainEntry.EntityType.DisplayName(),
                            mainEntry.BuildCurrentValuesString(mainEntry.EntityType.FindPrimaryKey()!.Properties),
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

        /// <summary>
        ///     Creates a new <see cref="IColumnModification" /> and add it to this command.
        /// </summary>
        /// <param name="columnModificationParameters"> Creation parameters. </param>
        /// <returns> The new <see cref="IColumnModification" /> instance. </returns>
        public virtual IColumnModification AddColumnModification(ColumnModificationParameters columnModificationParameters)
        {
            var modification = CreateColumnModification(columnModificationParameters);

            if (_columnModifications == null)
            {
                _columnModifications = new();
            }

            _columnModifications.Add(modification);

            return modification;
        }

        /// <summary>
        ///     Creates a new instance that implements <see cref="IColumnModification" /> interface.
        /// </summary>
        /// <param name="columnModificationParameters"> Creation parameters. </param>
        /// <returns> The new instance that implements <see cref="IColumnModification" /> interface. </returns>
        protected virtual IColumnModification CreateColumnModification(ColumnModificationParameters columnModificationParameters)
            => new ColumnModification(columnModificationParameters);

        private List<IColumnModification> GenerateColumnModifications()
        {
            var state = EntityState;
            var adding = state == EntityState.Added;
            var updating = state == EntityState.Modified;
            var columnModifications = new List<IColumnModification>();
            Dictionary<string, ColumnValuePropagator>? sharedTableColumnMap = null;

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

                var optionalDependentWithAllNull =
                    (entry.EntityState == EntityState.Deleted
                        || entry.EntityState == EntityState.Added)
                        && tableMapping.Table.IsOptional(entry.EntityType)
                        && tableMapping.Table.GetRowInternalForeignKeys(entry.EntityType).Any();

                foreach (var columnMapping in tableMapping.ColumnMappings)
                {
                    var property = columnMapping.Property;
                    var column = (IColumn)columnMapping.Column;
                    var isKey = property.IsPrimaryKey();
                    var isCondition = !adding && (isKey || property.IsConcurrencyToken);
                    var readValue = state != EntityState.Deleted && entry.IsStoreGenerated(property);

                    ColumnValuePropagator? columnPropagator = null;
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

                        var columnModificationParameters = new ColumnModificationParameters(
                            entry,
                            property,
                            column,
                            _generateParameterName!,
                            columnMapping.TypeMapping,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            _sensitiveLoggingEnabled);

                        var columnModification = CreateColumnModification(columnModificationParameters);

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

                        if (optionalDependentWithAllNull
                            && columnModification.IsWrite
                            && (entry.EntityState != EntityState.Added || columnModification.Value is not null))
                        {
                            optionalDependentWithAllNull = false;
                        }
                    }
                }

                if (optionalDependentWithAllNull && _logger != null)
                {
                    if (_sensitiveLoggingEnabled)
                    {
                        _logger.OptionalDependentWithAllNullPropertiesWarningSensitive(entry);
                    }
                    else
                    {
                        _logger.OptionalDependentWithAllNullPropertiesWarning(entry);
                    }
                }
            }

            return columnModifications;
        }

        private ITableMappingBase? GetTableMapping(IEntityType entityType)
        {
            ITableMappingBase? tableMapping = null;
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
        ///     propagates them back to into the appropriate <see cref="IColumnModification" />
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

            result += "(" + string.Join(", ", _columnModifications.Where(m => m.IsKey).Select(m => m.OriginalValue?.ToString())) + ")";
            return result;
        }

        private sealed class ColumnValuePropagator
        {
            private bool _write;
            private object? _originalValue;
            private object? _currentValue;

            public IColumnModification? ColumnModification { get; set; }

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
