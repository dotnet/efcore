// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalComplexPropertyBuilder
    : InternalPropertyBaseBuilder<IConventionComplexPropertyBuilder, ComplexProperty>, IConventionComplexPropertyBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalComplexPropertyBuilder(ComplexProperty metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override InternalComplexPropertyBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexTypeBuilder ComplexTypeBuilder
        => Metadata.ComplexType.Builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ComplexPropertySnapshot? Detach(ComplexProperty complexProperty)
    {
        var complexType = complexProperty.ComplexType;
        if (!complexProperty.IsInModel
            || !complexType.IsInModel)
        {
            return null;
        }

        var property = complexProperty.DeclaringType.FindDeclaredComplexProperty(complexProperty.Name);
        if (property == null)
        {
            return null;
        }

        var propertyBuilder = property.Builder;
        // Reset convention configuration
        propertyBuilder.IsRequired(null, ConfigurationSource.Convention);

        List<RelationshipSnapshot>? detachedRelationships = null;
        foreach (var relationshipToBeDetached in complexType.ContainingEntityType.GetDeclaredForeignKeys().ToList())
        {
            if (!relationshipToBeDetached.Properties.Any(p => p.DeclaringType == complexType))
            {
                continue;
            }

            detachedRelationships ??= [];

            var detachedRelationship = InternalEntityTypeBuilder.DetachRelationship(relationshipToBeDetached, false);
            if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                || relationshipToBeDetached.IsOwnership)
            {
                detachedRelationships.Add(detachedRelationship);
            }
        }

        List<(InternalKeyBuilder, ConfigurationSource?)>? detachedKeys = null;
        foreach (var keyToDetach in complexType.ContainingEntityType.GetDeclaredKeys().ToList())
        {
            if (!keyToDetach.Properties.Any(p => p.DeclaringType == complexType))
            {
                continue;
            }

            foreach (var relationshipToBeDetached in keyToDetach.GetReferencingForeignKeys().ToList())
            {
                if (!relationshipToBeDetached.IsInModel
                    || !relationshipToBeDetached.DeclaringEntityType.IsInModel)
                {
                    // Referencing type might have been removed while removing other foreign keys
                    continue;
                }

                detachedRelationships ??= [];

                var detachedRelationship = InternalEntityTypeBuilder.DetachRelationship(relationshipToBeDetached, true);
                if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                    || relationshipToBeDetached.IsOwnership)
                {
                    detachedRelationships.Add(detachedRelationship);
                }
            }

            if (!keyToDetach.IsInModel)
            {
                continue;
            }

            detachedKeys ??= [];

            var detachedKey = InternalEntityTypeBuilder.DetachKey(keyToDetach);
            if (detachedKey.Item1.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
            {
                detachedKeys.Add(detachedKey);
            }
        }

        List<InternalIndexBuilder>? detachedIndexes = null;
        foreach (var indexToBeDetached in complexType.ContainingEntityType.GetDeclaredIndexes().ToList())
        {
            if (!indexToBeDetached.Properties.Any(p => p.DeclaringType == complexType))
            {
                continue;
            }

            detachedIndexes ??= [];

            var detachedIndex = InternalEntityTypeBuilder.DetachIndex(indexToBeDetached);
            if (detachedIndex.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
            {
                detachedIndexes.Add(detachedIndex);
            }
        }

        var detachedProperties = InternalTypeBaseBuilder.DetachProperties(complexType.GetDeclaredProperties().ToList());

        var snapshot = new ComplexPropertySnapshot(
            complexProperty.Builder,
            detachedProperties,
            detachedIndexes,
            detachedKeys,
            detachedRelationships);

        complexProperty.DeclaringType.RemoveComplexProperty(complexProperty);

        return snapshot;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
    {
        if (configurationSource != ConfigurationSource.Explicit
            && !CanSetIsRequired(required, configurationSource))
        {
            return null;
        }

        Metadata.SetIsNullable(!required, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsRequired(bool? required, ConfigurationSource? configurationSource)
        => (configurationSource.HasValue
                && configurationSource.Value.Overrides(Metadata.GetIsNullableConfigurationSource()))
            || (Metadata.IsNullable == !required);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBase IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.Metadata
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
    IConventionComplexProperty IConventionComplexPropertyBuilder.Metadata
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
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionComplexPropertyBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionComplexPropertyBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
        => (IConventionComplexPropertyBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexPropertyBuilder? IConventionComplexPropertyBuilder.IsRequired(bool? required, bool fromDataAnnotation)
        => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionComplexPropertyBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
        => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.HasField(
        string? fieldName,
        bool fromDataAnnotation)
        => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.HasField(
        FieldInfo? fieldInfo,
        bool fromDataAnnotation)
        => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.CanSetField(string? fieldName, bool fromDataAnnotation)
        => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => UsePropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>.CanSetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
