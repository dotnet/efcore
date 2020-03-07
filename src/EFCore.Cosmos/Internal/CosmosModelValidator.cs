// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosModelValidator : ModelValidator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosModelValidator([NotNull] ModelValidatorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.Validate(model, logger);

            ValidateSharedContainerCompatibility(model, logger);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateSharedContainerCompatibility(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var containers = new Dictionary<string, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
            {
                var container = entityType.GetContainer();
                if (container == null)
                {
                    continue;
                }

                if (!containers.TryGetValue(container, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    containers[container] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var containerMapping in containers)
            {
                var mappedTypes = containerMapping.Value;
                var container = containerMapping.Key;
                ValidateSharedContainerCompatibility(mappedTypes, container, logger);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateSharedContainerCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string container,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var discriminatorValues = new Dictionary<object, IEntityType>();
            IProperty partitionKey = null;
            IEntityType firstEntityType = null;
            foreach (var entityType in mappedTypes)
            {
                var partitionKeyPropertyName = entityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName != null)
                {
                    var nextPartitionKeyProperty = entityType.FindProperty(partitionKeyPropertyName);
                    if (nextPartitionKeyProperty == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyMissingProperty(entityType.DisplayName(), partitionKeyPropertyName));
                    }

                    var keyType = nextPartitionKeyProperty.GetTypeMapping().Converter?.ProviderClrType
                                  ?? nextPartitionKeyProperty.ClrType;
                    if (keyType != typeof(string))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyNonStringStoreType(
                                partitionKeyPropertyName, entityType.DisplayName(), keyType.ShortDisplayName()));
                    }

                    if (partitionKey == null)
                    {
                        if (firstEntityType != null)
                        {
                            throw new InvalidOperationException(CosmosStrings.NoPartitionKey(firstEntityType.DisplayName(), container));
                        }

                        partitionKey = nextPartitionKeyProperty;
                    }
                    else if (partitionKey.GetPropertyName() != nextPartitionKeyProperty.GetPropertyName())
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyStoreNameMismatch(
                                partitionKey.Name, firstEntityType.DisplayName(), partitionKey.GetPropertyName(),
                                nextPartitionKeyProperty.Name, entityType.DisplayName(), nextPartitionKeyProperty.GetPropertyName()));
                    }
                }
                else if (partitionKey != null)
                {
                    throw new InvalidOperationException(CosmosStrings.NoPartitionKey(entityType.DisplayName(), container));
                }

                if (mappedTypes.Count == 1)
                {
                    break;
                }

                if (firstEntityType == null)
                {
                    firstEntityType = entityType;
                }

                if (entityType.ClrType?.IsInstantiable() == true
                    && entityType.GetContainingPropertyName() == null)
                {
                    if (entityType.GetDiscriminatorProperty() == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NoDiscriminatorProperty(entityType.DisplayName(), container));
                    }

                    var discriminatorValue = entityType.GetDiscriminatorValue();
                    if (discriminatorValue == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NoDiscriminatorValue(entityType.DisplayName(), container));
                    }

                    if (discriminatorValues.TryGetValue(discriminatorValue, out var duplicateEntityType))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.DuplicateDiscriminatorValue(
                                entityType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName(), container));
                    }

                    discriminatorValues[discriminatorValue] = entityType;
                }
            }
        }
    }
}
