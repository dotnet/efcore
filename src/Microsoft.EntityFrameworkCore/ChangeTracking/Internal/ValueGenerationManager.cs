// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ValueGenerationManager : IValueGenerationManager
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IKeyPropagator _keyPropagator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ValueGenerationManager(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IKeyPropagator keyPropagator)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
            _keyPropagator = keyPropagator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Generate(InternalEntityEntry entry)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var propertyTuple in FindProperties(entry))
            {
                var property = propertyTuple.Item1;

                if (propertyTuple.Item2)
                {
                    _keyPropagator.PropagateValue(entry, property);
                }
                else
                {
                    var valueGenerator = GetValueGenerator(entry, property);

                    SetGeneratedValue(
                        entry,
                        property,
                        valueGenerator.Next(entityEntry),
                        valueGenerator.GeneratesTemporaryValues);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task GenerateAsync(
            InternalEntityEntry entry,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var propertyTuple in FindProperties(entry))
            {
                var property = propertyTuple.Item1;

                if (propertyTuple.Item2)
                {
                    _keyPropagator.PropagateValue(entry, property);
                }
                else
                {
                    var valueGenerator = GetValueGenerator(entry, property);

                    SetGeneratedValue(
                        entry,
                        property,
                        await valueGenerator.NextAsync(entityEntry, cancellationToken),
                        valueGenerator.GeneratesTemporaryValues);
                }
            }
        }

        private IEnumerable<Tuple<IProperty, bool>> FindProperties(InternalEntityEntry entry)
        {
            foreach (var property in entry.EntityType.GetProperties())
            {
                var isForeignKey = property.IsForeignKey();

                if ((property.RequiresValueGenerator || isForeignKey)
                    && property.ClrType.IsDefaultValue(entry[property]))
                {
                    yield return Tuple.Create(property, isForeignKey);
                }
            }
        }

        private ValueGenerator GetValueGenerator(InternalEntityEntry entry, IProperty property)
            => _valueGeneratorSelector.Select(property, property.IsKey()
                ? property.DeclaringEntityType
                : entry.EntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool MayGetTemporaryValue(IProperty property, IEntityType entityType)
            => property.RequiresValueGenerator
               && _valueGeneratorSelector.Select(property, entityType).GeneratesTemporaryValues;

        private static void SetGeneratedValue(InternalEntityEntry entry, IProperty property, object generatedValue, bool isTemporary)
        {
            if (generatedValue != null)
            {
                entry[property] = generatedValue;

                if (isTemporary)
                {
                    entry.MarkAsTemporary(property);
                }
            }
        }
    }
}
