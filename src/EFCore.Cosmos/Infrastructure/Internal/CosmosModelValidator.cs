// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

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
    public CosmosModelValidator(ModelValidatorDependencies dependencies)
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

    /// <inheritdoc />
    protected override void ValidatePrimitiveCollections(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            Validate(entityType, logger);
        }

        static void Validate(ITypeBase typeBase, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in typeBase.GetDeclaredProperties())
            {
                if (property is { IsPrimitiveCollection: true })
                {
                    throw new InvalidOperationException(
                        CosmosStrings.PrimitiveCollectionsNotSupported(
                            property.DeclaringType.ClrType.ShortDisplayName(),
                            property.Name));
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                Validate(complexProperty.ComplexType, logger);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateSharedContainerCompatibility(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var containers = new Dictionary<string, List<IEntityType>>();
        foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
        {
            var container = entityType.GetContainer();
            if (container == null)
            {
                continue;
            }

            var ownership = entityType.FindOwnership();
            if (ownership != null)
            {
                throw new InvalidOperationException(
                    CosmosStrings.OwnedTypeDifferentContainer(
                        entityType.DisplayName(), ownership.PrincipalEntityType.DisplayName(), container));
            }

            if (entityType.GetContainingPropertyName() != null)
            {
                throw new InvalidOperationException(
                    CosmosStrings.ContainerContainingPropertyConflict(
                        entityType.DisplayName(), container, entityType.GetContainingPropertyName()));
            }

            if (!containers.TryGetValue(container, out var mappedTypes))
            {
                mappedTypes = [];
                containers[container] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }

        foreach (var (container, mappedTypes) in containers)
        {
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
        IReadOnlyList<IEntityType> mappedTypes,
        string container,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var discriminatorValues = new Dictionary<object, IEntityType>();
        IProperty? partitionKey = null;
        int? analyticalTtl = null;
        int? defaultTtl = null;
        ThroughputProperties? throughput = null;
        IEntityType? firstEntityType = null;
        foreach (var entityType in mappedTypes)
        {
            Check.DebugAssert(entityType.IsDocumentRoot(), "Only document roots expected here.");
            var partitionKeyPropertyName = entityType.GetPartitionKeyPropertyName();
            if (partitionKeyPropertyName != null)
            {
                var nextPartitionKeyProperty = entityType.FindProperty(partitionKeyPropertyName)!;
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
                            partitionKey.Name, firstEntityType!.DisplayName(), partitionKey.GetJsonPropertyName(),
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

            firstEntityType ??= entityType;

            if (entityType.ClrType.IsInstantiable()
                && entityType.GetContainingPropertyName() == null)
            {
                if (entityType.FindDiscriminatorProperty() == null)
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

            var currentAnalyticalTtl = entityType.GetAnalyticalStoreTimeToLive();
            if (currentAnalyticalTtl != null)
            {
                if (analyticalTtl == null)
                {
                    analyticalTtl = currentAnalyticalTtl;
                }
                else if (analyticalTtl != currentAnalyticalTtl)
                {
                    var conflictingEntityType = mappedTypes.First(et => et.GetAnalyticalStoreTimeToLive() != null);
                    throw new InvalidOperationException(
                        CosmosStrings.AnalyticalTTLMismatch(
                            analyticalTtl, conflictingEntityType.DisplayName(), entityType.DisplayName(), currentAnalyticalTtl,
                            container));
                }
            }

            var currentDefaultTtl = entityType.GetDefaultTimeToLive();
            if (currentDefaultTtl != null)
            {
                if (defaultTtl == null)
                {
                    defaultTtl = currentDefaultTtl;
                }
                else if (defaultTtl != currentDefaultTtl)
                {
                    var conflictingEntityType = mappedTypes.First(et => et.GetDefaultTimeToLive() != null);
                    throw new InvalidOperationException(
                        CosmosStrings.DefaultTTLMismatch(
                            defaultTtl, conflictingEntityType.DisplayName(), entityType.DisplayName(), currentDefaultTtl, container));
                }
            }

            var currentThroughput = entityType.GetThroughput();
            if (currentThroughput != null)
            {
                if (throughput == null)
                {
                    throughput = currentThroughput;
                }
                else if ((throughput.AutoscaleMaxThroughput ?? throughput.Throughput)
                         != (currentThroughput.AutoscaleMaxThroughput ?? currentThroughput.Throughput))
                {
                    var conflictingEntityType = mappedTypes.First(et => et.GetThroughput() != null);
                    throw new InvalidOperationException(
                        CosmosStrings.ThroughputMismatch(
                            throughput.AutoscaleMaxThroughput ?? throughput.Throughput, conflictingEntityType.DisplayName(),
                            entityType.DisplayName(), currentThroughput.AutoscaleMaxThroughput ?? currentThroughput.Throughput,
                            container));
                }
                else if ((throughput.AutoscaleMaxThroughput == null)
                         != (currentThroughput.AutoscaleMaxThroughput == null))
                {
                    var conflictingEntityType = mappedTypes.First(et => et.GetThroughput() != null);
                    var autoscaleType = throughput.AutoscaleMaxThroughput == null
                        ? entityType
                        : conflictingEntityType;
                    var manualType = throughput.AutoscaleMaxThroughput != null
                        ? entityType
                        : conflictingEntityType;

                    throw new InvalidOperationException(
                        CosmosStrings.ThroughputTypeMismatch(
                            manualType.DisplayName(), autoscaleType.DisplayName(), container));
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
    protected virtual void ValidateOnlyETagConcurrencyToken(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
    ///     Validates the mapping/configuration of mutable in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateNoMutableKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
                if (mutableProperty != null
                    && !mutableProperty.IsOrdinalKeyProperty())
                {
                    throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
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
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

                var jsonName = navigation.TargetEntityType.GetContainingPropertyName()!;
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
