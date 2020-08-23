// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Utilities;

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

            ValidateDatabaseProperties(model, logger);
            ValidateKeys(model, logger);
            ValidateSharedContainerCompatibility(model, logger);
            ValidateOnlyETagConcurrencyToken(model, logger);
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
                Check.DebugAssert(entityType.IsDocumentRoot(), "Only document roots expected here.");
                var partitionKeyPropertyName = entityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName != null)
                {
                    var nextPartitionKeyProperty = entityType.FindProperty(partitionKeyPropertyName);
                    if (partitionKey == null)
                    {
                        if (firstEntityType != null)
                        {
                            throw new InvalidOperationException(CosmosStrings.NoPartitionKey(firstEntityType.DisplayName(), container));
                        }

                        partitionKey = nextPartitionKeyProperty;
                    }
                    else if (partitionKey.GetJsonPropertyName() != nextPartitionKeyProperty.GetJsonPropertyName())
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyStoreNameMismatch(
                                partitionKey.Name, firstEntityType.DisplayName(), partitionKey.GetJsonPropertyName(),
                                nextPartitionKeyProperty.Name, entityType.DisplayName(), nextPartitionKeyProperty.GetJsonPropertyName()));
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateOnlyETagConcurrencyToken(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsConcurrencyToken)
                    {
                        var storeName = property.GetJsonPropertyName();
                        if (storeName != "_etag")
                        {
                            throw new InvalidOperationException(
                                CosmosStrings.NonETagConcurrencyToken(entityType.DisplayName(), storeName));
                        }

                        var etagType = property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType;
                        if (etagType != typeof(string))
                        {
                            throw new InvalidOperationException(
                                CosmosStrings.ETagNonStringStoreType(
                                    property.Name, entityType.DisplayName(), etagType.ShortDisplayName()));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateKeys(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey == null
                    || !entityType.IsDocumentRoot())
                {
                    continue;
                }

                var idProperty = entityType.GetProperties()
                    .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                if (idProperty == null)
                {
                    throw new InvalidOperationException(CosmosStrings.NoIdProperty(entityType.DisplayName()));
                }

                var idType = idProperty.GetTypeMapping().Converter?.ProviderClrType
                    ?? idProperty.ClrType;
                if (idType != typeof(string))
                {
                    throw new InvalidOperationException(
                        CosmosStrings.IdNonStringStoreType(
                            idProperty.Name, entityType.DisplayName(), idType.ShortDisplayName()));
                }

                if (!idProperty.IsKey())
                {
                    throw new InvalidOperationException(CosmosStrings.NoIdKey(entityType.DisplayName(), idProperty.Name));
                }

                var partitionKeyPropertyName = entityType.GetPartitionKeyPropertyName();
                if (partitionKeyPropertyName != null)
                {
                    var partitionKey = entityType.FindProperty(partitionKeyPropertyName);
                    if (partitionKey == null)
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyMissingProperty(entityType.DisplayName(), partitionKeyPropertyName));
                    }

                    var partitionKeyType = partitionKey.GetTypeMapping().Converter?.ProviderClrType
                        ?? partitionKey.ClrType;
                    if (partitionKeyType != typeof(string))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.PartitionKeyNonStringStoreType(
                                partitionKeyPropertyName, entityType.DisplayName(), partitionKeyType.ShortDisplayName()));
                    }

                    if (!partitionKey.GetContainingKeys().Any(k => k.Properties.Contains(idProperty)))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NoPartitionKeyKey(
                                entityType.DisplayName(), partitionKeyPropertyName, idProperty.Name));
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateDatabaseProperties(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                var properties = new Dictionary<string, IPropertyBase>();
                foreach (var property in entityType.GetProperties())
                {
                    var jsonName = property.GetJsonPropertyName();
                    if (string.IsNullOrWhiteSpace(jsonName))
                    {
                        continue;
                    }

                    if (properties.TryGetValue(jsonName, out var otherProperty))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.JsonPropertyCollision(property.Name, otherProperty.Name, entityType.DisplayName(), jsonName));
                    }

                    properties[jsonName] = property;
                }

                foreach (var navigation in entityType.GetNavigations())
                {
                    if (!navigation.IsEmbedded())
                    {
                        continue;
                    }

                    var jsonName = navigation.TargetEntityType.GetContainingPropertyName();
                    if (properties.TryGetValue(jsonName, out var otherProperty))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.JsonPropertyCollision(navigation.Name, otherProperty.Name, entityType.DisplayName(), jsonName));
                    }

                    properties[jsonName] = navigation;
                }
            }
        }
    }
}
