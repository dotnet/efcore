// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommand
    {
        private readonly ParameterNameGenerator _parameterNameGenerator;
        private readonly string _tableName;
        private readonly List<StateEntry> _stateEntries = new List<StateEntry>();

        private readonly LazyRef<IReadOnlyList<ColumnModification>> _columnModifications
            = new LazyRef<IReadOnlyList<ColumnModification>>(() => new ColumnModification[0]);

        private bool _requiresResultPropagation;

        public ModificationCommand([NotNull] string tableName, [NotNull] ParameterNameGenerator parameterNameGenerator)
        {
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(parameterNameGenerator, "parameterNameGenerator");

            _tableName = tableName;
            _parameterNameGenerator = parameterNameGenerator;
        }

        public virtual string TableName
        {
            get { return _tableName; }
        }

        public virtual IReadOnlyList<StateEntry> StateEntries
        {
            get { return _stateEntries; }
        }

        public virtual ModificationCommand AddStateEntry([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            if (!stateEntry.EntityState.IsDirty())
            {
                throw new NotSupportedException(Strings.FormatModificationFunctionInvalidEntityState(stateEntry.EntityState));
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

        public virtual EntityState EntityState
        {
            get
            {
                var firstEntry = _stateEntries.FirstOrDefault();

                return firstEntry != null ? firstEntry.EntityState : EntityState.Unknown;
            }
        }

        public virtual IReadOnlyList<ColumnModification> ColumnModifications
        {
            get { return _columnModifications.Value; }
        }

        public virtual bool RequiresResultPropagation
        {
            get
            {
                var _ = _columnModifications.Value;
                return _requiresResultPropagation;
            }
        }

        private IReadOnlyList<ColumnModification> GenerateColumnModifications()
        {
            var adding = EntityState == EntityState.Added;
            var deleting = EntityState == EntityState.Deleted;
            var columnModifications = new List<ColumnModification>();

            foreach (var stateEntry in _stateEntries)
            {
                var entityType = stateEntry.EntityType;

                foreach (var property in entityType.Properties)
                {
                    // TODO: Concurrency columns
                    var isKey = entityType.GetKey().Properties.Contains(property);
                    var isCondition = isKey || (!adding && property.IsConcurrencyToken);

                    var readValue = !deleting && (property.ValueGenerationStrategy == ValueGenerationStrategy.StoreComputed
                                    || (adding && property.ValueGenerationStrategy == ValueGenerationStrategy.StoreIdentity));

                    // TODO: Default values
                    // TODO: Should not need to filter key values here but they currently can get marked as modified
                    var writeValue = (adding && property.ValueGenerationStrategy != ValueGenerationStrategy.StoreComputed
                                      && property.ValueGenerationStrategy != ValueGenerationStrategy.StoreIdentity)
                                     || (!isKey && !deleting && stateEntry.IsPropertyModified(property));

                    if (readValue
                        || writeValue
                        || isKey
                        || isCondition)
                    {
                        if (readValue)
                        {
                            _requiresResultPropagation = true;
                        }

                        columnModifications.Add(new ColumnModification(
                            stateEntry, property, _parameterNameGenerator.GenerateNext(), readValue, writeValue, isKey, isCondition));
                    }
                }
            }
            return columnModifications;
        }

        public virtual void PropagateResults([NotNull] IValueReader reader)
        {
            Check.NotNull(reader, "reader");

            // TODO: Consider using strongly typed ReadValue instead of just <object>
            // Note that this call sets the value into a sidecar and will only commit to the actual entity
            // if SaveChanges is successful.
            var columnOperations = ColumnModifications.Where(o => o.IsRead).ToArray();
            for (var i = 0; i < columnOperations.Length; i++)
            {
                columnOperations[i].StateEntry[columnOperations[i].Property] = reader.ReadValue<object>(i);
            }
        }
    }
}
