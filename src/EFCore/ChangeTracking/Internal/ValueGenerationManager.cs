// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class ValueGenerationManager : IValueGenerationManager
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IKeyPropagator _keyPropagator;
        private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
        private readonly ILoggingOptions _loggingOptions;
        private readonly Dictionary<IEntityType, List<IProperty>> _entityTypePropagatingPropertiesMap
            = new Dictionary<IEntityType, List<IProperty>>();
        private readonly Dictionary<IEntityType, List<IProperty>> _entityTypeGeneratingPropertiesMap
            = new Dictionary<IEntityType, List<IProperty>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry Propagate(InternalEntityEntry entry)
        {
            InternalEntityEntry chosenPrincipal = null;
            foreach (var property in FindCandidatePropagatingProperties(entry))
            {
                if (!entry.HasDefaultValue(property))
                {
                    continue;
                }

                var principalEntry = _keyPropagator.PropagateValue(entry, property);
                if (chosenPrincipal == null)
                {
                    chosenPrincipal = principalEntry;
                }
            }

            return chosenPrincipal;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Generate(InternalEntityEntry entry, bool includePrimaryKey = true)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in FindCandidateGeneratingProperties(entry))
            {
                if (!entry.HasDefaultValue(property)
                    || (!includePrimaryKey
                        && property.IsPrimaryKey()))
                {
                    continue;
                }

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task GenerateAsync(
            InternalEntityEntry entry,
            bool includePrimaryKey = true,
            CancellationToken cancellationToken = default)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in FindCandidateGeneratingProperties(entry))
            {
                if (!entry.HasDefaultValue(property)
                    || (!includePrimaryKey
                        && property.IsPrimaryKey()))
                {
                    continue;
                }

                var valueGenerator = GetValueGenerator(entry, property);
                var generatedValue = await valueGenerator.NextAsync(entityEntry, cancellationToken)
                    .ConfigureAwait(false);
                var temporary = valueGenerator.GeneratesTemporaryValues;

                Log(entry, property, generatedValue, temporary);

                SetGeneratedValue(
                    entry,
                    property,
                    generatedValue,
                    temporary);
            }
        }

        private List<IProperty> FindCandidatePropagatingProperties(InternalEntityEntry entry)
        {
            if (!_entityTypePropagatingPropertiesMap.TryGetValue(entry.EntityType, out var candidateProperties))
            {
                candidateProperties = new List<IProperty>();
                foreach (var property in entry.EntityType.GetProperties())
                {
                    if (property.IsForeignKey())
                    {
                        candidateProperties.Add(property);
                    }
                }

                _entityTypePropagatingPropertiesMap[entry.EntityType] = candidateProperties;
            }

            return candidateProperties;
        }

        private List<IProperty> FindCandidateGeneratingProperties(InternalEntityEntry entry)
        {
            if (!_entityTypeGeneratingPropertiesMap.TryGetValue(entry.EntityType, out var candidateProperties))
            {
                candidateProperties = new List<IProperty>();
                foreach (var property in entry.EntityType.GetProperties())
                {
                    if (property.RequiresValueGenerator())
                    {
                        candidateProperties.Add(property);
                    }
                }

                _entityTypeGeneratingPropertiesMap[entry.EntityType] = candidateProperties;
            }

            return candidateProperties;
        }

        private ValueGenerator GetValueGenerator(InternalEntityEntry entry, IProperty property)
            => _valueGeneratorSelector.Select(
                property, property.IsKey()
                    ? property.DeclaringEntityType
                    : entry.EntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
