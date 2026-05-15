// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalIndexBuilder : AnnotatableBuilder<Index, InternalModelBuilder>, IConventionIndexBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalIndexBuilder(Index index, InternalModelBuilder modelBuilder)
        : base(index, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? IsUnique(bool? unique, ConfigurationSource configurationSource)
    {
        if (!CanSetIsUnique(unique, configurationSource))
        {
            return null;
        }

        Metadata.SetIsUnique(unique, configurationSource);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource)
        => Metadata.IsUnique == unique
            || configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? IsDescending(IReadOnlyList<bool>? descending, ConfigurationSource configurationSource)
    {
        if (!CanSetIsDescending(descending, configurationSource))
        {
            return null;
        }

        Metadata.SetIsDescending(descending, configurationSource);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsDescending(IReadOnlyList<bool>? descending, ConfigurationSource? configurationSource)
        => descending is null && Metadata.IsDescending is null
            || descending is not null && Metadata.IsDescending is not null && Metadata.IsDescending.SequenceEqual(descending)
            || configurationSource.Overrides(Metadata.GetIsDescendingConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var configurationSource = Metadata.GetConfigurationSource();
        InternalIndexBuilder? newIndexBuilder;

        // If the index targets complex / chained properties or carries collection indices, we can't
        // simply re-resolve PropertyBase instances at the entity-type level — the leaves may live
        // inside complex types that were themselves rebuilt during detach. Reconstruct the segment
        // lists plus per-leaf collection indices from the original Index and go through the
        // HasIndex overload that resolves the chain against the current model.
        if (RequiresComplexReattach(Metadata, out var namesPerLeaf, out var isCollection, out var collectionIndices))
        {
            var properties = entityTypeBuilder.GetOrCreateProperties(namesPerLeaf, isCollection, configurationSource);
            newIndexBuilder = properties is null
                ? null
                : entityTypeBuilder.HasIndex(properties, collectionIndices, Metadata.Name, configurationSource);
        }
        else
        {
            var properties = entityTypeBuilder.GetActualProperties(Metadata.Properties, null);
            if (properties == null)
            {
                return null;
            }

            newIndexBuilder = Metadata.Name == null
                ? entityTypeBuilder.HasIndex(properties, configurationSource)
                : entityTypeBuilder.HasIndex(properties, Metadata.Name, configurationSource);
        }

        newIndexBuilder?.MergeAnnotationsFrom(Metadata);

        var isUniqueConfigurationSource = Metadata.GetIsUniqueConfigurationSource();
        if (isUniqueConfigurationSource.HasValue)
        {
            newIndexBuilder?.IsUnique(Metadata.IsUnique, isUniqueConfigurationSource.Value);
        }

        return newIndexBuilder;
    }

    private static bool RequiresComplexReattach(
        Index index,
        out IReadOnlyList<IReadOnlyList<string>> namesPerLeaf,
        out IReadOnlyList<IReadOnlyList<bool>>? isCollection,
        out IReadOnlyList<IReadOnlyList<int?>?>? collectionIndices)
    {
        var indexCollectionIndices = index.CollectionIndices;
        var properties = index.Properties;
        var propertyCount = properties.Count;

        var anyComplexChain = false;
        for (var i = 0; i < propertyCount; i++)
        {
            if (properties[i].DeclaringType is ComplexType)
            {
                anyComplexChain = true;
                break;
            }
        }

        if (!anyComplexChain && indexCollectionIndices is null)
        {
            namesPerLeaf = [];
            isCollection = null;
            collectionIndices = null;
            return false;
        }

        var chains = new IReadOnlyList<string>[propertyCount];
        var allFlags = new IReadOnlyList<bool>[propertyCount];

        for (var i = 0; i < propertyCount; i++)
        {
            var property = properties[i];

            // Measure the chain depth first so we can size arrays exactly and fill in reverse order,
            // avoiding the cost of List<>.Reverse() and List<> capacity doubling.
            var depth = 0;
            var declaringType = property.DeclaringType;
            while (declaringType is ComplexType walking)
            {
                depth++;
                declaringType = walking.ComplexProperty.DeclaringType;
            }

            var chainNames = new string[depth + 1];
            var chainFlags = new bool[depth + 1];
            chainNames[depth] = property.Name;
            // The leaf entry of chainFlags stays false: the indexed leaf is the property itself,
            // not a collection-traversal step on the way to it.
            declaringType = property.DeclaringType;
            for (var pos = depth - 1; pos >= 0; pos--)
            {
                var complexType = (ComplexType)declaringType!;
                chainNames[pos] = complexType.ComplexProperty.Name;
                chainFlags[pos] = complexType.ComplexProperty.IsCollection;
                declaringType = complexType.ComplexProperty.DeclaringType;
            }

            chains[i] = chainNames;
            allFlags[i] = chainFlags;
        }

        namesPerLeaf = chains;
        // We're already on the slow path (anyComplexChain is true), so always emit the flag list so the
        // consumer can reconstruct each chain with the correct collection / non-collection structure.
        isCollection = allFlags;
        collectionIndices = indexCollectionIndices;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionIndex IConventionIndexBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionIndexBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionIndexBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionIndexBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionIndexBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionIndexBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionIndexBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionIndexBuilder? IConventionIndexBuilder.IsUnique(bool? unique, bool fromDataAnnotation)
        => IsUnique(
            unique,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionIndexBuilder.CanSetIsUnique(bool? unique, bool fromDataAnnotation)
        => CanSetIsUnique(
            unique,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionIndexBuilder? IConventionIndexBuilder.IsDescending(IReadOnlyList<bool>? descending, bool fromDataAnnotation)
        => IsDescending(
            descending,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionIndexBuilder.CanSetIsDescending(IReadOnlyList<bool>? descending, bool fromDataAnnotation)
        => CanSetIsDescending(
            descending,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
