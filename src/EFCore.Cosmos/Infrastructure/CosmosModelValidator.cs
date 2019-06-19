// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure
{
    public class CosmosModelValidator : ModelValidator
    {
        public CosmosModelValidator([NotNull] ModelValidatorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.Validate(model, logger);

            ValidateSharedContainerCompatibility(model, logger);
        }

        /// <summary>
        ///     Validates the mapping/configuration of shared containers in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedContainerCompatibility(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var containers = new Dictionary<string, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
            {
                var containerName = entityType.GetCosmosContainerName();

                if (!containers.TryGetValue(containerName, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    containers[containerName] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var containerMapping in containers)
            {
                var mappedTypes = containerMapping.Value;
                var containerName = containerMapping.Key;
                ValidateSharedContainerCompatibility(mappedTypes, containerName, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of entity types sharing a given container.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="containerName"> The container name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedContainerCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string containerName,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (mappedTypes.Count == 1)
            {
                var entityType = mappedTypes[0];
                var partitionKeyPropertyName = entityType.GetCosmosPartitionKeyPropertyName();
                if (partitionKeyPropertyName != null)
                {
                    var nextPartitionKeyProperty = entityType.FindProperty(partitionKeyPropertyName);
                    if (nextPartitionKeyProperty == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyMissingProperty(entityType.DisplayName(), partitionKeyPropertyName));
                    }
                }
                return;
            }

            var discriminatorValues = new Dictionary<object, IEntityType>();
            IProperty partitionKey = null;
            IEntityType firstEntityType = null;
            foreach (var entityType in mappedTypes)
            {
                var partitionKeyPropertyName = entityType.GetCosmosPartitionKeyPropertyName();
                if (partitionKeyPropertyName != null)
                {
                    var nextPartitionKeyProperty = entityType.FindProperty(partitionKeyPropertyName);
                    if (nextPartitionKeyProperty == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyMissingProperty(entityType.DisplayName(), partitionKeyPropertyName));
                    }

                    if (partitionKey == null)
                    {
                        if (firstEntityType != null)
                        {
                            throw new InvalidOperationException(CosmosStrings.NoPartitionKey(firstEntityType.DisplayName(), containerName));
                        }
                        partitionKey = nextPartitionKeyProperty;
                    }
                    else if (partitionKey.GetCosmosPropertyName() != nextPartitionKeyProperty.GetCosmosPropertyName())
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyStoreNameMismatch(
                                partitionKey.Name, firstEntityType.DisplayName(), partitionKey.GetCosmosPropertyName(),
                                nextPartitionKeyProperty.Name, entityType.DisplayName(), nextPartitionKeyProperty.GetCosmosPropertyName()));
                    }
                    else if ((partitionKey.FindMapping().Converter?.ProviderClrType ?? partitionKey.ClrType)
                      != (nextPartitionKeyProperty.FindMapping().Converter?.ProviderClrType ?? nextPartitionKeyProperty.ClrType))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyStoreTypeMismatch(
                                partitionKey.Name,
                                firstEntityType.DisplayName(),
                                (partitionKey.FindMapping().Converter?.ProviderClrType ?? partitionKey.ClrType).ShortDisplayName(),
                                nextPartitionKeyProperty.Name,
                                entityType.DisplayName(),
                                (nextPartitionKeyProperty.FindMapping().Converter?.ProviderClrType ?? nextPartitionKeyProperty.ClrType)
                                    .ShortDisplayName()));
                    }
                }
                else if (partitionKey != null)
                {
                    throw new InvalidOperationException(CosmosStrings.NoPartitionKey(entityType.DisplayName(), containerName));
                }

                if (firstEntityType == null)
                {
                    firstEntityType = entityType;
                }

                if (entityType.ClrType?.IsInstantiable() == true)
                {
                    if (entityType.GetDiscriminatorProperty() == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NoDiscriminatorProperty(entityType.DisplayName(), containerName));
                    }

                    var discriminatorValue = entityType.GetDiscriminatorValue();
                    if (discriminatorValue == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NoDiscriminatorValue(entityType.DisplayName(), containerName));
                    }

                    if (discriminatorValues.TryGetValue(discriminatorValue, out var duplicateEntityType))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.DuplicateDiscriminatorValue(
                                entityType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName(), containerName));
                    }

                    discriminatorValues[discriminatorValue] = entityType;
                }
            }
        }
    }
}
