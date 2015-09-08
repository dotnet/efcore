// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Update
{
    public class ModificationCommand
    {
        private readonly Func<IProperty, IRelationalPropertyAnnotations> _getPropertyExtensions;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly List<InternalEntityEntry> _entries = new List<InternalEntityEntry>();

        private readonly LazyRef<IReadOnlyList<ColumnModification>> _columnModifications
            = new LazyRef<IReadOnlyList<ColumnModification>>(() => new ColumnModification[0]);

        private readonly LazyRef<IRelationalValueBufferFactory> _valueBufferFactory;

        private bool _requiresResultPropagation;

        public ModificationCommand(
            [NotNull] string name,
            [CanBeNull] string schemaName,
            [NotNull] ParameterNameGenerator parameterNameGenerator,
            [NotNull] Func<IProperty, IRelationalPropertyAnnotations> getPropertyExtensions,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(parameterNameGenerator, nameof(parameterNameGenerator));
            Check.NotNull(getPropertyExtensions, nameof(getPropertyExtensions));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            TableName = name;
            SchemaName = schemaName;
            ParameterNameGenerator = parameterNameGenerator;
            _getPropertyExtensions = getPropertyExtensions;
            _valueBufferFactoryFactory = valueBufferFactoryFactory;

            _valueBufferFactory = new LazyRef<IRelationalValueBufferFactory>(CreateValueBufferFactory);
        }

        public virtual string TableName { get; }

        public virtual string SchemaName { get; }

        public virtual IReadOnlyList<InternalEntityEntry> Entries => _entries;

        public virtual EntityState EntityState => _entries.FirstOrDefault()?.EntityState ?? EntityState.Detached;

        public virtual IReadOnlyList<ColumnModification> ColumnModifications => _columnModifications.Value;

        public virtual IRelationalValueBufferFactory ValueBufferFactory => _valueBufferFactory.Value;

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
            _valueBufferFactory.Reset(CreateValueBufferFactory);

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
                            readValue,
                            writeValue,
                            isKey,
                            isCondition));
                    }
                }
            }

            return columnModifications;
        }

        private IRelationalValueBufferFactory CreateValueBufferFactory()
            => _valueBufferFactoryFactory
                .Create(
                    ColumnModifications
                        .Where(c => c.IsRead)
                        .Select(c => c.Property.ClrType)
                        .ToArray(),
                    indexMap: null);

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
