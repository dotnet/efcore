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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class ModificationCommand
    {
        private readonly Func<IProperty, IRelationalPropertyAnnotations> _getPropertyExtensions;
        private readonly Func<string> _generateParameterName;

        private readonly List<IUpdateEntry> _entries = new List<IUpdateEntry>();
        private IReadOnlyList<ColumnModification> _columnModifications;
        private bool _requiresResultPropagation;

        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<string> generateParameterName,
            [NotNull] Func<IProperty, IRelationalPropertyAnnotations> getPropertyExtensions)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(generateParameterName, nameof(generateParameterName));
            Check.NotNull(getPropertyExtensions, nameof(getPropertyExtensions));

            TableName = name;
            Schema = schema;
            _generateParameterName = generateParameterName;
            _getPropertyExtensions = getPropertyExtensions;
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
                // ReSharper disable once UnusedVariable
                var _ = ColumnModifications;
                return _requiresResultPropagation;
            }
        }

        public virtual void AddEntry([NotNull] IUpdateEntry entry)
        {
            Check.NotNull(entry, nameof(entry));

            if ((entry.EntityState != EntityState.Added)
                && (entry.EntityState != EntityState.Modified)
                && (entry.EntityState != EntityState.Deleted))
            {
                throw new ArgumentException(RelationalStrings.ModificationFunctionInvalidEntityState(entry.EntityState));
            }

            var firstEntry = _entries.FirstOrDefault();
            if ((firstEntry != null)
                && (firstEntry.EntityState != entry.EntityState))
            {
                throw new InvalidOperationException(RelationalStrings.ConflictingRowUpdates);

                // TODO: Check for any other conflicts between the two entries
            }

            _entries.Add(entry);
            _columnModifications = null;
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var adding = EntityState == EntityState.Added;
            var columnModifications = new List<ColumnModification>();

            foreach (var entry in _entries)
            {
                var entityType = entry.EntityType;

                foreach (var property in entityType.GetProperties())
                {
                    var isKey = property.IsPrimaryKey();
                    var isConcurrencyToken = property.IsConcurrencyToken;
                    var isCondition = !adding && (isKey || isConcurrencyToken);
                    var readValue = entry.IsStoreGenerated(property);
                    var writeValue = !readValue && (adding || entry.IsModified(property));

                    if (adding
                        && !readValue
                        && entry.HasTemporaryValue(property))
                    {
                        throw new InvalidOperationException(CoreStrings.TempValue(property.Name, entityType.DisplayName()));
                    }

                    if (readValue
                        || writeValue
                        || isCondition)
                    {
                        if (readValue)
                        {
                            _requiresResultPropagation = true;
                        }

                        columnModifications.Add(new ColumnModification(
                            entry,
                            property,
                            _getPropertyExtensions(property),
                            _generateParameterName,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition,
                            isConcurrencyToken));
                    }
                }
            }

            return columnModifications;
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
