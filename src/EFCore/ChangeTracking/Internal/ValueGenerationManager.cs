// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
        private readonly ILoggingOptions _loggingOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ValueGenerationManager(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IKeyPropagator keyPropagator,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
            [NotNull] ILoggingOptions loggingOptions)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
            _keyPropagator = keyPropagator;
            _logger = logger;
            _loggingOptions = loggingOptions;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry Propagate(InternalEntityEntry entry)
        {
            InternalEntityEntry chosenPrincipal = null;
            foreach (var property in FindPropagatingProperties(entry))
            {
                var principalEntry = _keyPropagator.PropagateValue(entry, property);
                if (chosenPrincipal == null)
                {
                    chosenPrincipal = principalEntry;
                }
            }

            return chosenPrincipal;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Generate(InternalEntityEntry entry)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in FindGeneratingProperties(entry))
            {
                var valueGenerator = GetValueGenerator(entry, property);

                var generatedValue = valueGenerator.Next(entityEntry);
                var temporary = valueGenerator.GeneratesTemporaryValues;

                Log(entry, property, generatedValue, temporary);

                SetGeneratedValue(
                    entry,
                    property,
                    generatedValue,
                    temporary);
            }
        }

        private void Log(InternalEntityEntry entry, IProperty property, object generatedValue, bool temporary)
        {
            if (_loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _logger.ValueGeneratedSensitive(entry, property, generatedValue, temporary);
            }
            else
            {
                _logger.ValueGenerated(entry, property, generatedValue, temporary);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task GenerateAsync(
            InternalEntityEntry entry,
            CancellationToken cancellationToken = default)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in FindGeneratingProperties(entry))
            {
                var valueGenerator = GetValueGenerator(entry, property);
                var generatedValue = await valueGenerator.NextAsync(entityEntry, cancellationToken);
                var temporary = valueGenerator.GeneratesTemporaryValues;

                Log(entry, property, generatedValue, temporary);

                SetGeneratedValue(
                    entry,
                    property,
                    generatedValue,
                    temporary);
            }
        }

        private static IEnumerable<IProperty> FindPropagatingProperties(InternalEntityEntry entry)
        {
            foreach (var property in ((EntityType)entry.EntityType).GetProperties())
            {
                if (property.IsForeignKey()
                    && entry.HasDefaultValue(property))
                {
                    yield return property;
                }
            }
        }

        private static IEnumerable<IProperty> FindGeneratingProperties(InternalEntityEntry entry)
        {
            foreach (var property in ((EntityType)entry.EntityType).GetProperties())
            {
                if (property.RequiresValueGenerator()
                    && entry.HasDefaultValue(property))
                {
                    yield return property;
                }
            }
        }

        private ValueGenerator GetValueGenerator(InternalEntityEntry entry, IProperty property)
            => _valueGeneratorSelector.Select(
                property, property.IsKey()
                    ? property.DeclaringEntityType
                    : entry.EntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool MayGetTemporaryValue(IProperty property, IEntityType entityType)
            => property.RequiresValueGenerator()
               && _valueGeneratorSelector.Select(property, entityType).GeneratesTemporaryValues;

        private static void SetGeneratedValue(InternalEntityEntry entry, IProperty property, object generatedValue, bool isTemporary)
        {
            if (generatedValue != null)
            {
                if (isTemporary)
                {
                    entry.SetTemporaryValue(property, generatedValue);
                }
                else
                {
                    entry[property] = generatedValue;
                }
            }
        }
    }
}
