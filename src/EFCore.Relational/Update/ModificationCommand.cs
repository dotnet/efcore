// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
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
                null)
        {
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            _generateParameterName = generateParameterName;
            _comparer = comparer;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
        }

        /// <summary>
        ///     Initializes a new <see cref="ModificationCommand" /> instance.
        /// </summary>
        /// <param name="name"> The name of the table containing the data to be modified. </param>
        /// <param name="schema"> The schema containing the table, or <c>null</c> to use the default schema. </param>
        /// <param name="columnModifications"> The list of <see cref="ColumnModification" />s needed to perform the insert, update, or delete. </param>
        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schema,
            [CanBeNull] IReadOnlyList<ColumnModification> columnModifications)
        {
            Check.NotNull(name, nameof(name));

            TableName = name;
            Schema = schema;
            _columnModifications = columnModifications;
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
                foreach (var e in _entries)
                {
                    if (e.SharedIdentityEntry == null)
                    {
                        return e.EntityState;
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
                var lastEntry = _entries[_entries.Count - 1];
                var lastEntryState = lastEntry.SharedIdentityEntry == null
                    ? lastEntry.EntityState
                    : EntityState.Modified;
                var entryState = entry.SharedIdentityEntry == null
                    ? entry.EntityState
                    : EntityState.Modified;
                if (lastEntryState != entryState)
                {
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ConflictingRowUpdateTypesSensitive(
                                entry.EntityType.DisplayName(),
                                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                                entryState,
                                lastEntry.EntityType.DisplayName(),
                                lastEntry.BuildCurrentValuesString(lastEntry.EntityType.FindPrimaryKey().Properties),
                                lastEntryState));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowUpdateTypes(
                            entry.EntityType.DisplayName(),
                            entryState,
                            lastEntry.EntityType.DisplayName(),
                            lastEntryState));
                }
            }

            _entries.Add(entry);
            _columnModifications = null;
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var adding = EntityState == EntityState.Added;
            var columnModifications = new List<ColumnModification>();

            if (_comparer != null)
            {
                _entries.Sort(_comparer);
            }

            var columnMap = _entries.Count == 1
                ? null
                : new Dictionary<string, ColumnModification>();

            Dictionary<IUpdateEntry, List<ColumnModification>> conflictingColumnValues = null;
            Dictionary<IUpdateEntry, List<ColumnModification>> conflictingOriginalColumnValues = null;

            foreach (var entry in _entries)
            {
                Dictionary<string, IProperty> sharedIdentityEntryProperties = null;
                if (entry.SharedIdentityEntry != null)
                {
                    if (entry.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    sharedIdentityEntryProperties = new Dictionary<string, IProperty>();

                    foreach (var property in entry.SharedIdentityEntry.EntityType.GetProperties())
                    {
                        sharedIdentityEntryProperties[property.Relational().ColumnName] = property;
                    }
                }

                foreach (var property in entry.EntityType.GetProperties())
                {
                    var propertyAnnotations = property.Relational();
                    var isKey = property.IsPrimaryKey();
                    var isConcurrencyToken = property.IsConcurrencyToken;
                    var isCondition = !adding && (isKey || isConcurrencyToken);
                    var readValue = entry.IsStoreGenerated(property);

                    var writeValue = false;
                    if (!readValue)
                    {
                        if (adding)
                        {
                            writeValue = property.BeforeSaveBehavior == PropertySaveBehavior.Save;
                        }
                        else
                        {
                            if (property.AfterSaveBehavior == PropertySaveBehavior.Save
                                && entry.IsModified(property))
                            {
                                writeValue = true;
                            }
                            else if (sharedIdentityEntryProperties != null
                                     && (property.BeforeSaveBehavior == PropertySaveBehavior.Save
                                         || property.AfterSaveBehavior == PropertySaveBehavior.Save))
                            {
                                writeValue = !sharedIdentityEntryProperties.TryGetValue(propertyAnnotations.ColumnName, out var originalProperty)
                                             || !Equals(entry.SharedIdentityEntry.GetOriginalValue(originalProperty), entry.GetCurrentValue(property));
                            }
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
                            propertyAnnotations,
                            _generateParameterName,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            isConcurrencyToken);

                        if (columnMap != null)
                        {
                            if (columnMap.TryGetValue(columnModification.ColumnName, out var existingColumnModification))
                            {
                                if (columnModification.UseCurrentValueParameter
                                    && !Equals(columnModification.Value, existingColumnModification.Value))
                                {
                                    conflictingColumnValues = AddConflictingColumnValues(
                                        conflictingColumnValues, columnModification, existingColumnModification);
                                }
                                else if (columnModification.UseOriginalValueParameter
                                         && !Equals(columnModification.OriginalValue, existingColumnModification.OriginalValue))
                                {
                                    conflictingOriginalColumnValues = AddConflictingColumnValues(
                                        conflictingOriginalColumnValues, columnModification, existingColumnModification);
                                }

                                continue;
                            }

                            columnMap.Add(columnModification.ColumnName, columnModification);
                        }

                        columnModifications.Add(columnModification);
                    }
                }
            }

            if (conflictingColumnValues != null)
            {
                var firstPair = conflictingColumnValues.First();
                var firstEntry = firstPair.Key;
                var firstProperties = firstPair.Value.Select(c => c.Property).ToList();
                var lastPair = conflictingColumnValues.Last();
                var lastEntry = lastPair.Key;
                var lastProperties = lastPair.Value.Select(c => c.Property);

                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowValuesSensitive(
                            firstEntry.EntityType.DisplayName(),
                            lastEntry.EntityType.DisplayName(),
                            firstEntry.BuildCurrentValuesString(firstEntry.EntityType.FindPrimaryKey().Properties),
                            firstEntry.BuildCurrentValuesString(firstProperties),
                            lastEntry.BuildCurrentValuesString(lastProperties),
                            firstProperties.FormatColumns()));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowValues(
                        firstEntry.EntityType.DisplayName(),
                        lastEntry.EntityType.DisplayName(),
                        Property.Format(firstProperties),
                        Property.Format(lastProperties),
                        firstProperties.FormatColumns()));
            }

            if (conflictingOriginalColumnValues != null)
            {
                var firstPair = conflictingOriginalColumnValues.First();
                var firstEntry = firstPair.Key;
                var firstProperties = firstPair.Value.Select(c => c.Property).ToList();
                var lastPair = conflictingOriginalColumnValues.Last();
                var lastEntry = lastPair.Key;
                var lastProperties = lastPair.Value.Select(c => c.Property);

                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingOriginalRowValuesSensitive(
                            firstEntry.EntityType.DisplayName(),
                            lastEntry.EntityType.DisplayName(),
                            firstEntry.BuildCurrentValuesString(firstEntry.EntityType.FindPrimaryKey().Properties),
                            firstEntry.BuildOriginalValuesString(firstProperties),
                            lastEntry.BuildOriginalValuesString(lastProperties),
                            firstProperties.FormatColumns()));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingOriginalRowValues(
                        firstEntry.EntityType.DisplayName(),
                        lastEntry.EntityType.DisplayName(),
                        Property.Format(firstProperties),
                        Property.Format(lastProperties),
                        firstProperties.FormatColumns()));
            }

            return columnModifications;
        }

        private static Dictionary<IUpdateEntry, List<ColumnModification>> AddConflictingColumnValues(
            Dictionary<IUpdateEntry, List<ColumnModification>> conflictingColumnValues,
            ColumnModification columnModification,
            ColumnModification existingColumn)
        {
            if (conflictingColumnValues == null)
            {
                conflictingColumnValues = new Dictionary<IUpdateEntry, List<ColumnModification>>();
            }

            if (!conflictingColumnValues.TryGetValue(columnModification.Entry, out var conflictList))
            {
                conflictList = new List<ColumnModification>();
                conflictingColumnValues.Add(columnModification.Entry, conflictList);
            }

            conflictList.Add(columnModification);

            if (!conflictingColumnValues.TryGetValue(existingColumn.Entry, out var otherConflictList))
            {
                otherConflictList = new List<ColumnModification>();
                conflictingColumnValues.Add(existingColumn.Entry, otherConflictList);
            }

            otherConflictList.Add(existingColumn);

            return conflictingColumnValues;
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
    }
}
