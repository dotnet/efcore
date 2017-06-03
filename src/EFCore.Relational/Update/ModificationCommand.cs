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
    public class ModificationCommand
    {
        private readonly Func<string> _generateParameterName;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly IComparer<IUpdateEntry> _comparer;

        private readonly List<IUpdateEntry> _entries = new List<IUpdateEntry>();
        private IReadOnlyList<ColumnModification> _columnModifications;
        private bool _requiresResultPropagation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            [CanBeNull] IComparer<IUpdateEntry> comparer)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            TableName = name;
            Schema = schema;
            _generateParameterName = generateParameterName;
            _comparer = comparer;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
        }

        public virtual string TableName { get; }

        public virtual string Schema { get; }

        public virtual IReadOnlyList<IUpdateEntry> Entries => _entries;

        public virtual EntityState EntityState => _entries.FirstOrDefault()?.EntityState ?? EntityState.Detached;

        public virtual IReadOnlyList<ColumnModification> ColumnModifications
            => NonCapturingLazyInitializer.EnsureInitialized(ref _columnModifications, this, command => command.GenerateColumnModifications());

        public virtual bool RequiresResultPropagation
        {
            get
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = ColumnModifications;

                return _requiresResultPropagation;
            }
        }

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
                if (lastEntry.EntityState != entry.EntityState)
                {
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ConflictingRowUpdateTypesSensitive(
                                entry.EntityType.DisplayName(),
                                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                                entry.EntityState,
                                lastEntry.EntityType.DisplayName(),
                                lastEntry.BuildCurrentValuesString(lastEntry.EntityType.FindPrimaryKey().Properties),
                                lastEntry.EntityState));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowUpdateTypes(
                            entry.EntityType.DisplayName(),
                            entry.EntityState,
                            lastEntry.EntityType.DisplayName(),
                            lastEntry.EntityState));
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
                var entityType = entry.EntityType;

                foreach (var property in entityType.GetProperties())
                {
                    var isKey = property.IsPrimaryKey();
                    var isConcurrencyToken = property.IsConcurrencyToken;
                    var isCondition = !adding && (isKey || isConcurrencyToken);
                    var readValue = entry.IsStoreGenerated(property);

                    var writeValue = !readValue
                                     && (adding
                                         && property.BeforeSaveBehavior == PropertySaveBehavior.Save
                                         || !adding
                                         && property.AfterSaveBehavior == PropertySaveBehavior.Save
                                         && entry.IsModified(property));

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
                            property.Relational(),
                            _generateParameterName,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            isConcurrencyToken);

                        if (columnMap != null)
                        {
                            if (columnMap.TryGetValue(columnModification.ColumnName, out var existingColumn))
                            {
                                if (columnModification.UseCurrentValueParameter
                                    && !Equals(columnModification.Value, existingColumn.Value))
                                {
                                    conflictingColumnValues = AddConflictingColumnValues(
                                        conflictingColumnValues, columnModification, existingColumn);
                                }
                                else if (columnModification.UseOriginalValueParameter
                                         && !Equals(columnModification.OriginalValue, existingColumn.OriginalValue))
                                {
                                    conflictingOriginalColumnValues = AddConflictingColumnValues(
                                        conflictingOriginalColumnValues, columnModification, existingColumn);
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
