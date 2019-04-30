// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        /// <summary>
        ///     Initializes a new <see cref="ModificationCommand" /> instance.
        /// </summary>
        /// <param name="name"> The name of the table containing the data to be modified. </param>
        /// <param name="schema"> The schema containing the table, or <c>null</c> to use the default schema. </param>
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
        /// <param name="schema"> The schema containing the table, or <c>null</c> to use the default schema. </param>
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
        ///     The schema containing the table, or <c>null</c> to use the default schema.
        /// </summary>
        public virtual string Schema { get; }

        /// <summary>
        ///     The <see cref="IUpdateEntry" />s that represent the entities that are mapped to the row
        ///     to update.
        /// </summary>
        public virtual IReadOnlyList<IUpdateEntry> Entries => _entries;

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
                if (_entries.Count > 0)
                {
                    for (var i = 0; i < _entries.Count; i++)
                    {
                        var entry = _entries[0];
                        if (entry.SharedIdentityEntry != null)
                        {
                            return EntityState.Modified;
                        }

                        var state = entry.EntityState;
                        if (state != EntityState.Unchanged)
                        {
                            return state;
                        }
                    }
                }

                return EntityState.Modified;
            }
        }

        /// <summary>
        ///     The list of <see cref="ColumnModification" />s needed to perform the insert, update, or delete.
        /// </summary>
        public virtual IReadOnlyList<ColumnModification> ColumnModifications
            => NonCapturingLazyInitializer.EnsureInitialized(ref _columnModifications, this, command => command.GenerateColumnModifications());

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
        public virtual void AddEntry([NotNull] IUpdateEntry entry)
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

            if (_entries.Count > 0)
            {
                var currentState = EntityState;
                var entryState = entry.SharedIdentityEntry == null
                    ? entry.EntityState
                    : EntityState.Modified;
                if (currentState != entryState
                    && entryState != EntityState.Unchanged)
                {
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ConflictingRowUpdateTypesSensitive(
                                entry.EntityType.DisplayName(),
                                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                                entryState,
                                _entries[0].EntityType.DisplayName(),
                                _entries[0].BuildCurrentValuesString(_entries[0].EntityType.FindPrimaryKey().Properties),
                                currentState));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowUpdateTypes(
                            entry.EntityType.DisplayName(),
                            entryState,
                            _entries[0].EntityType.DisplayName(),
                            currentState));
                }
            }

            _entries.Add(entry);
            _columnModifications = null;
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var state = EntityState;
            var adding = state == EntityState.Added;
            var updating = state == EntityState.Modified;
            var columnModifications = new List<ColumnModification>();
            Dictionary<string, ColumnValuePropagator> sharedColumnMap = null;

            if (_entries.Count > 1
                || (_entries.Count == 1 && _entries[0].SharedIdentityEntry != null))
            {
                sharedColumnMap = new Dictionary<string, ColumnValuePropagator>();

                if (_comparer != null)
                {
                    _entries.Sort(_comparer);
                }

                foreach (var entry in _entries)
                {
                    if (entry.SharedIdentityEntry != null)
                    {
                        InitializeSharedColumns(entry.SharedIdentityEntry, updating, sharedColumnMap);
                    }

                    InitializeSharedColumns(entry, updating, sharedColumnMap);
                }
            }

            foreach (var entry in _entries)
            {
                foreach (var property in entry.EntityType.GetProperties())
                {
                    var isKey = property.IsPrimaryKey();
                    var isConcurrencyToken = property.IsConcurrencyToken;
                    var isCondition = !adding && (isKey || isConcurrencyToken);
                    var readValue = entry.IsStoreGenerated(property);
                    var columnName = property.GetColumnName();
                    var columnPropagator = sharedColumnMap?[columnName];

                    var writeValue = false;
                    if (!readValue)
                    {
                        if (adding)
                        {
                            writeValue = property.GetBeforeSaveBehavior() == PropertySaveBehavior.Save;
                        }
                        else if (updating && property.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
                        {
                            writeValue = columnPropagator?.TryPropagate(property, (InternalEntityEntry)entry)
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
                            _generateParameterName,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            isConcurrencyToken,
                            _sensitiveLoggingEnabled);

                        if (columnPropagator != null)
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

        private static void InitializeSharedColumns(IUpdateEntry entry, bool updating, Dictionary<string, ColumnValuePropagator> columnMap)
        {
            foreach (var property in entry.EntityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!columnMap.TryGetValue(columnName, out var columnPropagator))
                {
                    columnPropagator = new ColumnValuePropagator();
                    columnMap.Add(columnName, columnPropagator);
                }

                if (updating)
                {
                    columnPropagator.RecordValue(property, entry);
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

        private class ColumnValuePropagator
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
                        if (!_write && entry.IsModified(property))
                        {
                            _write = true;
                            _currentValue = entry.GetCurrentValue(property);
                        }
                        break;
                    case EntityState.Added:
                        if (!_write)
                        {
                            _currentValue = entry.GetCurrentValue(property);
                            _write = !Equals(_originalValue, _currentValue);
                        }
                        break;
                    case EntityState.Deleted:
                        _originalValue = entry.GetOriginalValue(property);
                        break;
                }
            }

            public bool TryPropagate(IProperty property, InternalEntityEntry entry)
            {
                if (_write
                    && (entry.EntityState == EntityState.Unchanged
                       || (entry.EntityState == EntityState.Modified && !entry.IsModified(property))
                       || (entry.EntityState == EntityState.Added && Equals(_originalValue, entry.GetCurrentValue(property)))))
                {
                    entry[property] = _currentValue;

                    return false;
                }

                return _write;
            }
        }
    }
}
