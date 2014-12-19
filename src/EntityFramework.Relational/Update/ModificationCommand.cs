// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommand
    {
        private readonly Func<IProperty, IRelationalPropertyExtensions> _getPropertyExtensions;
        private readonly List<StateEntry> _stateEntries = new List<StateEntry>();

        private readonly LazyRef<IReadOnlyList<ColumnModification>> _columnModifications
            = new LazyRef<IReadOnlyList<ColumnModification>>(() => new ColumnModification[0]);

        private bool _requiresResultPropagation;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ModificationCommand()
        {
        }

        public ModificationCommand(
            SchemaQualifiedName schemaQualifiedName,
            [NotNull] ParameterNameGenerator parameterNameGenerator,
            [NotNull] Func<IProperty, IRelationalPropertyExtensions> getPropertyExtensions)
        {
            Check.NotEmpty(schemaQualifiedName, "schemaQualifiedName");
            Check.NotNull(parameterNameGenerator, "parameterNameGenerator");
            Check.NotNull(getPropertyExtensions, "getPropertyExtensions");

            SchemaQualifiedName = schemaQualifiedName;
            ParameterNameGenerator = parameterNameGenerator;
            _getPropertyExtensions = getPropertyExtensions;
        }

        public virtual SchemaQualifiedName SchemaQualifiedName { get; }

        public virtual IReadOnlyList<StateEntry> StateEntries => _stateEntries;

        public virtual EntityState EntityState => _stateEntries.FirstOrDefault()?.EntityState ?? EntityState.Unknown;

        public virtual IReadOnlyList<ColumnModification> ColumnModifications => _columnModifications.Value;

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

        public virtual ModificationCommand AddStateEntry([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            if (!stateEntry.EntityState.IsDirty())
            {
                throw new NotSupportedException(Strings.ModificationFunctionInvalidEntityState(stateEntry.EntityState));
            }

            var firstEntry = _stateEntries.FirstOrDefault();
            if (firstEntry != null
                && firstEntry.EntityState != stateEntry.EntityState)
            {
                // TODO: Proper message
                throw new InvalidOperationException("Two entities cannot make conflicting updates to the same row.");

                // TODO: Check for any other conflicts between the two entries
            }

            _stateEntries.Add(stateEntry);
            _columnModifications.Reset(GenerateColumnModifications);

            return this;
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var adding = EntityState == EntityState.Added;
            var columnModifications = new List<ColumnModification>();

            foreach (var stateEntry in _stateEntries)
            {
                var entityType = stateEntry.EntityType;

                foreach (var property in entityType.Properties)
                {
                    var isKey = property.IsPrimaryKey();
                    var isCondition = !adding && (isKey || property.IsConcurrencyToken);
                    var readValue = stateEntry.StoreMustGenerateValue(property);
                    var writeValue = !readValue && (adding || stateEntry.IsPropertyModified(property));

                    if (readValue
                        || writeValue
                        || isCondition)
                    {
                        if (readValue)
                        {
                            _requiresResultPropagation = true;
                        }

                        columnModifications.Add(new ColumnModification(
                            stateEntry,
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

        public virtual void PropagateResults([NotNull] IValueReader reader)
        {
            Check.NotNull(reader, "reader");

            // TODO: Consider using strongly typed ReadValue instead of just <object>
            // Issue #771
            // Note that this call sets the value into a sidecar and will only commit to the actual entity
            // if SaveChanges is successful.
            var columnOperations = ColumnModifications.Where(o => o.IsRead).ToArray();
            for (var i = 0; i < columnOperations.Length; i++)
            {
                columnOperations[i].Value = reader.IsNull(i) ? null : reader.ReadValue<object>(i);
            }
        }
    }
}
