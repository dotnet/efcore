// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommand
    {
        private readonly Func<IProperty, IRelationalPropertyExtensions> _getPropertyExtensions;
        private readonly IBoxedValueReaderSource _boxedValueReaderSource;
        private readonly List<InternalEntityEntry> _entries = new List<InternalEntityEntry>();

        private readonly LazyRef<IReadOnlyList<ColumnModification>> _columnModifications
            = new LazyRef<IReadOnlyList<ColumnModification>>(() => new ColumnModification[0]);

        private readonly IRelationalValueReaderFactory _valueReaderFactory;

        private bool _requiresResultPropagation;

        public ModificationCommand(
            [NotNull] string tableName,
            [CanBeNull] string schemaName,
            [NotNull] ParameterNameGenerator parameterNameGenerator,
            [NotNull] Func<IProperty, IRelationalPropertyExtensions> getPropertyExtensions,
            [NotNull] IBoxedValueReaderSource boxedValueReaderSource,
            [NotNull] IRelationalValueReaderFactoryFactory valueReaderFactoryFactory)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotNull(parameterNameGenerator, nameof(parameterNameGenerator));
            Check.NotNull(getPropertyExtensions, nameof(getPropertyExtensions));
            Check.NotNull(boxedValueReaderSource, nameof(boxedValueReaderSource));

            TableName = tableName;
            SchemaName = schemaName;
            ParameterNameGenerator = parameterNameGenerator;
            _getPropertyExtensions = getPropertyExtensions;
            _boxedValueReaderSource = boxedValueReaderSource;

            // TODO: Use type information to create value reader factory
            _valueReaderFactory = valueReaderFactoryFactory.CreateValueReaderFactory();
        }

        public virtual string TableName { get; }

        public virtual string SchemaName { get; }

        public virtual IReadOnlyList<InternalEntityEntry> Entries => _entries;

        public virtual EntityState EntityState => _entries.FirstOrDefault()?.EntityState ?? EntityState.Detached;

        public virtual IReadOnlyList<ColumnModification> ColumnModifications => _columnModifications.Value;

        public virtual IRelationalValueReaderFactory ValueReaderFactory => _valueReaderFactory;

        public virtual bool RequiresResultPropagation
        {
            get
            {
                // ReSharper disable once UnusedVariable
                var _ = _columnModifications.Value;
                return _requiresResultPropagation;
            }
        }

        public virtual ParameterNameGenerator ParameterNameGenerator { get; }

        public virtual ModificationCommand AddEntry([NotNull] InternalEntityEntry entry)
        {
            Check.NotNull(entry, nameof(entry));

            if (entry.EntityState != EntityState.Added
                && entry.EntityState != EntityState.Modified
                && entry.EntityState != EntityState.Deleted)
            {
                throw new NotSupportedException(Strings.ModificationFunctionInvalidEntityState(entry.EntityState));
            }

            var firstEntry = _entries.FirstOrDefault();
            if (firstEntry != null
                && firstEntry.EntityState != entry.EntityState)
            {
                // TODO: Proper message
                throw new InvalidOperationException("Two entities cannot make conflicting updates to the same row.");

                // TODO: Check for any other conflicts between the two entries
            }

            _entries.Add(entry);
            _columnModifications.Reset(GenerateColumnModifications);

            return this;
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
                    var isCondition = !adding && (isKey || property.IsConcurrencyToken);
                    var readValue = entry.StoreMustGenerateValue(property);
                    var writeValue = !readValue && (adding || entry.IsPropertyModified(property));

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
                            ParameterNameGenerator,
                            readValue ? _boxedValueReaderSource.GetReader(property) : null,
                            readValue,
                            writeValue,
                            isKey,
                            isCondition));
                    }
                }
            }

            return columnModifications;
        }

        public virtual void PropagateResults([NotNull] IValueReader reader)
        {
            Check.NotNull(reader, nameof(reader));

            // Note that this call sets the value into a sidecar and will only commit to the actual entity
            // if SaveChanges is successful.
            var index = 0;
            foreach (var modification in ColumnModifications.Where(o => o.IsRead))
            {
                modification.Value = modification.BoxedValueReader.ReadValue(reader, index++);
            }
        }
    }
}
