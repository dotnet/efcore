// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalEntityTypeBuilder : InternalTypeBaseBuilder, IConventionEntityTypeBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityTypeBuilder(EntityType metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual EntityType Metadata
        => (EntityType)base.Metadata;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? PrimaryKey(
        IReadOnlyList<string>? propertyNames,
        ConfigurationSource configurationSource)
        => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? PrimaryKey(
        IReadOnlyList<MemberInfo>? clrMembers,
        ConfigurationSource configurationSource)
        => PrimaryKey(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? PrimaryKey(
        IReadOnlyList<Property>? properties,
        ConfigurationSource configurationSource)
    {
        if (!CanSetPrimaryKey(properties, configurationSource))
        {
            return null;
        }

        InternalKeyBuilder? keyBuilder = null;
        if (properties == null)
        {
            Metadata.SetPrimaryKey(properties, configurationSource);
        }
        else
        {
            var previousPrimaryKey = Metadata.FindPrimaryKey();
            if (previousPrimaryKey != null
                && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
            {
                previousPrimaryKey.UpdateConfigurationSource(configurationSource);
                return Metadata.SetPrimaryKey(properties, configurationSource)!.Builder;
            }

            using (ModelBuilder.Metadata.DelayConventions())
            {
                keyBuilder = HasKeyInternal(properties, configurationSource);
                if (keyBuilder == null)
                {
                    return null;
                }

                var newKey = Metadata.SetPrimaryKey(keyBuilder.Metadata.Properties, configurationSource);
                foreach (var key in Metadata.GetDeclaredKeys().ToList())
                {
                    if (key == keyBuilder.Metadata
                        || !key.IsInModel)
                    {
                        continue;
                    }

                    var referencingForeignKeys = key
                        .GetReferencingForeignKeys()
                        .Where(fk => fk.GetPrincipalKeyConfigurationSource() == null)
                        .ToList();

                    foreach (var referencingForeignKey in referencingForeignKeys)
                    {
                        if (referencingForeignKey.GetPropertiesConfigurationSource() != null
                            && !ForeignKey.AreCompatible(
                                newKey!.Properties,
                                referencingForeignKey.Properties,
                                referencingForeignKey.PrincipalEntityType,
                                Metadata,
                                shouldThrow: false))
                        {
                            DetachRelationship(referencingForeignKey).Attach();
                        }
                        else
                        {
                            referencingForeignKey.Builder.HasPrincipalKey(
                                (IReadOnlyList<Property>?)null, ConfigurationSource.Convention);
                        }
                    }
                }

                if (previousPrimaryKey?.IsInModel == true)
                {
                    RemoveKeyIfUnused(previousPrimaryKey, configurationSource);
                }
            }
        }

        // TODO: Use convention batch to get the updated builder, see #15898
        if (keyBuilder is null || !keyBuilder.Metadata.IsInModel)
        {
            properties = GetActualProperties(properties, null);
            return properties == null ? null : Metadata.FindPrimaryKey(properties)!.Builder;
        }

        return keyBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetPrimaryKey(
        IReadOnlyList<string> propertyNames,
        ConfigurationSource configurationSource)
    {
        for (var i = 0; i < propertyNames.Count; i++)
        {
            if (!CanHaveProperty(
                    propertyType: null,
                    propertyNames[i],
                    null,
                    typeConfigurationSource: null,
                    configurationSource,
                    checkClrProperty: true))
            {
                return false;
            }
        }

        var previousPrimaryKey = Metadata.FindPrimaryKey();
        if (previousPrimaryKey != null
            && previousPrimaryKey.Properties.Select(p => p.Name).SequenceEqual(propertyNames))
        {
            return true;
        }

        return configurationSource.Overrides(Metadata.GetPrimaryKeyConfigurationSource())
            && (!Metadata.IsKeyless
                || configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetPrimaryKey(
        IReadOnlyList<IConventionProperty>? properties,
        ConfigurationSource configurationSource)
    {
        var previousPrimaryKey = Metadata.FindPrimaryKey();
        if (properties == null)
        {
            if (previousPrimaryKey == null)
            {
                return true;
            }
        }
        else if (previousPrimaryKey != null
                 && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
        {
            return true;
        }

        return configurationSource.Overrides(Metadata.GetPrimaryKeyConfigurationSource())
            && (properties == null
                || !Metadata.IsKeyless
                || configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? HasKey(IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? HasKey(IReadOnlyList<MemberInfo> clrMembers, ConfigurationSource configurationSource)
        => HasKeyInternal(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalKeyBuilder? HasKey(IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
        => HasKeyInternal(properties, configurationSource);

    private InternalKeyBuilder? HasKeyInternal(IReadOnlyList<Property>? properties, ConfigurationSource? configurationSource)
    {
        if (properties == null)
        {
            return null;
        }

        var actualProperties = GetActualProperties(properties, configurationSource)!;
        var key = Metadata.FindDeclaredKey(actualProperties);
        if (key == null)
        {
            if (configurationSource == null)
            {
                return null;
            }

            if (Metadata.IsKeyless
                && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()))
            {
                return null;
            }

            Metadata.SetIsKeyless(false, configurationSource.Value);

            var containingForeignKeys = actualProperties
                .SelectMany(p => p.GetContainingForeignKeys().Where(k => k.DeclaringEntityType != Metadata))
                .ToList();

            if (containingForeignKeys.Any(fk => !configurationSource.Overrides(fk.GetPropertiesConfigurationSource())))
            {
                return null;
            }

            if (configurationSource != ConfigurationSource.Explicit // let it throw for explicit
                && actualProperties.Any(p => !p.Builder.CanSetIsRequired(true, configurationSource)))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                foreach (var foreignKey in containingForeignKeys)
                {
                    if (foreignKey.GetPropertiesConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        // let it throw for explicit
                        continue;
                    }

                    foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>?)null, configurationSource.Value);
                }

                foreach (var actualProperty in actualProperties)
                {
                    // TODO: Use layering #15898
                    actualProperty.Builder.IsRequired(true, configurationSource.Value);
                }

                key = Metadata.AddKey(actualProperties, configurationSource.Value)!;
            }

            if (!key.IsInModel)
            {
                key = Metadata.FindDeclaredKey(actualProperties);
            }
        }
        else if (configurationSource.HasValue)
        {
            key.UpdateConfigurationSource(configurationSource.Value);
            Metadata.SetIsKeyless(false, configurationSource.Value);
        }

        return key?.IsInModel == true ? key.Builder : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoKey(Key key, ConfigurationSource configurationSource)
    {
        var currentConfigurationSource = key.GetConfigurationSource();
        if (!configurationSource.Overrides(currentConfigurationSource))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            var detachedRelationships = key.GetReferencingForeignKeys().ToList().Select(DetachRelationship).ToList();

            Metadata.RemoveKey(key);

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            RemoveUnusedImplicitProperties(key.Properties);
            foreach (var property in key.Properties)
            {
                if (!property.IsKey()
                    && property.ClrType.IsNullableType()
                    && !property.GetContainingForeignKeys().Any(fk => fk.IsRequired))
                {
                    // TODO: This should be handled by reference tracking, see #15898
                    if (property.IsInModel)
                    {
                        property.Builder.IsRequired(null, configurationSource);
                    }
                }
            }
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveKey(Key key, ConfigurationSource configurationSource)
        => configurationSource.Overrides(key.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static List<(InternalKeyBuilder, ConfigurationSource?)>? DetachKeys(IEnumerable<Key> keysToDetach)
    {
        var keysToDetachList = (keysToDetach as List<Key>) ?? keysToDetach.ToList();
        if (keysToDetachList.Count == 0)
        {
            return null;
        }

        var detachedKeys = new List<(InternalKeyBuilder, ConfigurationSource?)>();
        foreach (var keyToDetach in keysToDetachList)
        {
            var detachedKey = DetachKey(keyToDetach);
            detachedKeys.Add(detachedKey);
        }

        return detachedKeys;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static (InternalKeyBuilder, ConfigurationSource?) DetachKey(Key keyToDetach)
    {
        var entityTypeBuilder = keyToDetach.DeclaringEntityType.Builder;
        var keyBuilder = keyToDetach.Builder;

        var primaryKeyConfigurationSource = ((IReadOnlyKey)keyToDetach).IsPrimaryKey()
            ? keyToDetach.DeclaringEntityType.GetPrimaryKeyConfigurationSource()
            : null;

        entityTypeBuilder.HasNoKey(keyToDetach, keyToDetach.GetConfigurationSource());

        return (keyBuilder, primaryKeyConfigurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoKey(ConfigurationSource configurationSource)
    {
        if (Metadata.IsKeyless)
        {
            Metadata.SetIsKeyless(true, configurationSource);
            return this;
        }

        if (!CanRemoveKey(configurationSource))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            foreach (var foreignKey in Metadata.GetReferencingForeignKeys().ToList())
            {
                if (foreignKey.GetConfigurationSource() != ConfigurationSource.Explicit
                    || configurationSource != ConfigurationSource.Explicit)
                {
                    foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                    continue;
                }

                if (foreignKey.DependentToPrincipal != null && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationToKeylessType(foreignKey.DependentToPrincipal.Name, Metadata.DisplayName()));
                }
                else if ((foreignKey.IsUnique || foreignKey.GetIsUniqueConfigurationSource() != ConfigurationSource.Explicit)
                    && foreignKey.GetPrincipalEndConfigurationSource() != ConfigurationSource.Explicit
                    && foreignKey.Builder.CanSetEntityTypes(
                        foreignKey.DeclaringEntityType,
                        foreignKey.PrincipalEntityType,
                        configurationSource,
                        out _,
                        out var shouldResetToDependent)
                    && (!shouldResetToDependent || foreignKey.GetPrincipalToDependentConfigurationSource() != ConfigurationSource.Explicit))
                {
                    foreignKey.Builder.HasEntityTypes(
                        foreignKey.DeclaringEntityType,
                        foreignKey.PrincipalEntityType,
                        configurationSource);
                }
                else
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrincipalKeylessType(
                            Metadata.DisplayName(),
                            Metadata.DisplayName()
                            + (foreignKey.PrincipalToDependent == null
                                ? ""
                                : "." + foreignKey.PrincipalToDependent.Name),
                            foreignKey.DeclaringEntityType.DisplayName()));
                }
            }

            foreach (var foreignKey in Metadata.GetForeignKeys())
            {
                if (foreignKey.PrincipalToDependent == null)
                {
                    continue;
                }

                if (foreignKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                    && configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationToKeylessType(foreignKey.PrincipalToDependent.Name, Metadata.DisplayName()));
                }

                foreignKey.Builder.HasNavigation((string?)null, pointsToPrincipal: false, configurationSource);
            }

            foreach (var key in Metadata.GetKeys().ToList())
            {
                if (key.GetConfigurationSource() != ConfigurationSource.Explicit)
                {
                    HasNoKey(key, configurationSource);
                }
            }

            Metadata.SetIsKeyless(true, configurationSource);
            return this;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveKey(ConfigurationSource configurationSource)
        => Metadata.IsKeyless
            || (configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource())
                && Metadata.GetKeys().All(key => configurationSource.Overrides(key.GetConfigurationSource()))
                && Metadata.GetReferencingForeignKeys().All(fk => configurationSource.Overrides(fk.GetConfigurationSource()))
                && Metadata.GetForeignKeys().All(fk => configurationSource.Overrides(fk.GetPrincipalToDependentConfigurationSource())));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void RemoveMembersInHierarchy(string propertyName, ConfigurationSource configurationSource)
    {
        base.RemoveMembersInHierarchy(propertyName, configurationSource);

        foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(propertyName))
        {
            if (conflictingServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
            {
                conflictingServiceProperty.DeclaringEntityType.RemoveServiceProperty(conflictingServiceProperty);
            }
        }

        foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
        {
            if (conflictingNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        propertyName, Metadata.DisplayName(), conflictingNavigation.DeclaringEntityType.DisplayName()));
            }

            var foreignKey = conflictingNavigation.ForeignKey;
            if (foreignKey.GetConfigurationSource() == ConfigurationSource.Convention)
            {
                foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.Builder.HasNavigation(
                    (string?)null,
                    conflictingNavigation.IsOnDependent,
                    configurationSource);
            }
        }

        foreach (var conflictingSkipNavigation in Metadata.FindSkipNavigationsInHierarchy(propertyName))
        {
            if (conflictingSkipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
            {
                continue;
            }

            var inverse = conflictingSkipNavigation.Inverse;
            if (inverse?.IsInModel == true
                && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
            {
                inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource);
            }

            conflictingSkipNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(
                conflictingSkipNavigation, configurationSource);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool CanAddProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        ConfigurationSource configurationSource,
        bool checkClrProperty,
        bool skipTypeCheck)
        => !IsIgnored(propertyName, configurationSource)
            && (propertyType == null
                || skipTypeCheck
                || Metadata.Model.Builder.CanBeConfigured(propertyType, TypeConfigurationType.Property, configurationSource))
            && (!checkClrProperty
                || propertyType != null
                || Metadata.GetRuntimeProperties().ContainsKey(propertyName))
            && Metadata.FindServicePropertiesInHierarchy(propertyName).Cast<IConventionPropertyBase>()
                .Concat(Metadata.FindComplexPropertiesInHierarchy(propertyName))
                .Concat(Metadata.FindNavigationsInHierarchy(propertyName))
                .Concat(Metadata.FindSkipNavigationsInHierarchy(propertyName))
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IMutableNavigationBase Navigation(MemberInfo memberInfo)
        => Navigation(memberInfo.GetSimpleMemberName());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IMutableNavigationBase Navigation(string navigationName)
        => (IMutableNavigationBase?)Metadata.FindNavigation(navigationName)
            ?? Metadata.FindSkipNavigation(navigationName)
            ?? throw new InvalidOperationException(
                CoreStrings.CanOnlyConfigureExistingNavigations(navigationName, Metadata.DisplayName()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalServicePropertyBuilder? ServiceProperty(
        MemberInfo memberInfo,
        ConfigurationSource? configurationSource)
        => ServiceProperty(memberInfo.GetMemberType(), memberInfo, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalServicePropertyBuilder? ServiceProperty(
        Type serviceType,
        MemberInfo memberInfo,
        ConfigurationSource? configurationSource)
    {
        var propertyName = memberInfo.GetSimpleMemberName();
        List<ServiceProperty>? propertiesToDetach = null;
        InternalServicePropertyBuilder? builder;
        var existingProperty = Metadata.FindServiceProperty(propertyName);
        if (existingProperty != null)
        {
            if (existingProperty.DeclaringEntityType != Metadata)
            {
                if (!IsIgnored(propertyName, configurationSource))
                {
                    Metadata.RemoveIgnored(propertyName);
                }
            }

            if (existingProperty.GetIdentifyingMemberInfo()?.IsOverriddenBy(memberInfo) == true)
            {
                if (configurationSource.HasValue)
                {
                    existingProperty.UpdateConfigurationSource(configurationSource.Value);
                }

                return existingProperty.Builder;
            }

            if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
            {
                return null;
            }

            propertiesToDetach = [existingProperty];
        }
        else if (configurationSource != ConfigurationSource.Explicit
                 && (!configurationSource.HasValue
                     || !CanAddServiceProperty(memberInfo, configurationSource.Value)))
        {
            return null;
        }
        else
        {
            foreach (EntityType derivedType in Metadata.GetDerivedTypes())
            {
                var derivedProperty = derivedType.FindDeclaredServiceProperty(propertyName);
                if (derivedProperty != null)
                {
                    propertiesToDetach ??= [];

                    propertiesToDetach.Add(derivedProperty);
                }
            }
        }

        Check.DebugAssert(configurationSource is not null, "configurationSource is not null");

        using (ModelBuilder.Metadata.DelayConventions())
        {
            List<InternalServicePropertyBuilder>? detachedProperties = null;
            if (propertiesToDetach != null)
            {
                detachedProperties = [];
                foreach (var propertyToDetach in propertiesToDetach)
                {
                    detachedProperties.Add(DetachServiceProperty(propertyToDetach)!);
                }
            }

            if (existingProperty == null)
            {
                Metadata.RemoveIgnored(propertyName);

                RemoveMembersInHierarchy(propertyName, configurationSource.Value);
            }

            builder = Metadata.AddServiceProperty(memberInfo, serviceType, configurationSource.Value).Builder;

            if (detachedProperties != null)
            {
                foreach (var detachedProperty in detachedProperties)
                {
                    detachedProperty.Attach(this);
                }
            }
        }

        return builder.Metadata.IsInModel
            ? builder
            : Metadata.FindServiceProperty(propertyName)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveServiceProperty(MemberInfo memberInfo, ConfigurationSource? configurationSource)
    {
        var existingProperty = Metadata.FindServiceProperty(memberInfo);
        return existingProperty != null
            ? existingProperty.DeclaringType == Metadata
            || configurationSource.Overrides(existingProperty.GetConfigurationSource())
            : configurationSource.HasValue
            && CanAddServiceProperty(memberInfo, configurationSource.Value);
    }

    private bool CanAddServiceProperty(MemberInfo memberInfo, ConfigurationSource configurationSource)
    {
        var propertyName = memberInfo.GetSimpleMemberName();
        return !IsIgnored(propertyName, configurationSource)
            && Metadata.Model.Builder.CanBeConfigured(
                memberInfo.GetMemberType(), TypeConfigurationType.ServiceProperty, configurationSource)
            && Metadata.FindPropertiesInHierarchy(propertyName).Cast<IConventionPropertyBase>()
                .Concat(Metadata.FindComplexPropertiesInHierarchy(propertyName))
                .Concat(Metadata.FindNavigationsInHierarchy(propertyName))
                .Concat(Metadata.FindSkipNavigationsInHierarchy(propertyName))
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit)
            && Metadata.FindServicePropertiesInHierarchy(propertyName).All(
                m => (configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit)
                    || memberInfo.IsOverriddenBy(m.GetIdentifyingMemberInfo()));
    }

    private static InternalServicePropertyBuilder? DetachServiceProperty(ServiceProperty? serviceProperty)
    {
        if (serviceProperty is null || !serviceProperty.IsInModel)
        {
            return null;
        }

        var builder = serviceProperty.Builder;
        serviceProperty.DeclaringEntityType.RemoveServiceProperty(serviceProperty);
        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoServiceProperty(
        ServiceProperty serviceProperty,
        ConfigurationSource configurationSource)
    {
        if (!CanRemoveServiceProperty(serviceProperty, configurationSource))
        {
            return null;
        }

        Metadata.RemoveServiceProperty(serviceProperty);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveServiceProperty(ServiceProperty serviceProperty, ConfigurationSource configurationSource)
        => configurationSource.Overrides(serviceProperty.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool CanAddComplexProperty(
        string propertyName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? targetType,
        bool? collection,
        ConfigurationSource configurationSource,
        bool checkClrProperty = false)
        => !IsIgnored(propertyName, configurationSource)
            && (targetType == null || !ModelBuilder.IsIgnored(targetType, configurationSource))
            && (!checkClrProperty
                || propertyType != null
                || Metadata.GetRuntimeProperties().ContainsKey(propertyName))
            && Metadata.FindPropertiesInHierarchy(propertyName).Cast<IConventionPropertyBase>()
                .Concat(Metadata.FindServicePropertiesInHierarchy(propertyName))
                .Concat(Metadata.FindNavigationsInHierarchy(propertyName))
                .Concat(Metadata.FindSkipNavigationsInHierarchy(propertyName))
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveNavigation(
        string navigationName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? type,
        ConfigurationSource? configurationSource)
    {
        var existingNavigation = Metadata.FindNavigation(navigationName);
        return existingNavigation != null
            ? type == null
            || existingNavigation.ClrType == type
            || configurationSource.Overrides(existingNavigation.GetConfigurationSource())
            : configurationSource.HasValue
            && CanAddNavigation(navigationName, type, configurationSource.Value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanAddNavigation(
        string navigationName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? type,
        ConfigurationSource configurationSource)
        => !IsIgnored(navigationName, configurationSource)
            && (type == null || CanBeNavigation(type, configurationSource))
            && Metadata.FindPropertiesInHierarchy(navigationName).Cast<IConventionPropertyBase>()
                .Concat(Metadata.FindServicePropertiesInHierarchy(navigationName))
                .Concat(Metadata.FindComplexPropertiesInHierarchy(navigationName))
                .Concat(Metadata.FindSkipNavigationsInHierarchy(navigationName))
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

    private bool CanBeNavigation(Type type, ConfigurationSource configurationSource)
        => configurationSource == ConfigurationSource.Explicit
            || ModelBuilder.Metadata.Configuration?.GetConfigurationType(type).IsEntityType() != false
            && (type.TryGetSequenceType() is not Type sequenceType
                || ModelBuilder.Metadata.Configuration?.GetConfigurationType(sequenceType).IsEntityType() != false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveSkipNavigation(string skipNavigationName, Type? type, ConfigurationSource? configurationSource)
    {
        var existingNavigation = Metadata.FindSkipNavigation(skipNavigationName);
        return existingNavigation != null
            ? type == null
            || existingNavigation.ClrType == type
            || configurationSource.Overrides(existingNavigation.GetConfigurationSource())
            : configurationSource.HasValue
            && CanAddSkipNavigation(skipNavigationName, type, configurationSource.Value);
    }

    private bool CanAddSkipNavigation(
        string skipNavigationName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? type,
        ConfigurationSource configurationSource)
        => !IsIgnored(skipNavigationName, configurationSource)
            && (type == null || CanBeNavigation(type, configurationSource))
            && Metadata.FindPropertiesInHierarchy(skipNavigationName).Cast<IConventionPropertyBase>()
                .Concat(Metadata.FindComplexPropertiesInHierarchy(skipNavigationName))
                .Concat(Metadata.FindServicePropertiesInHierarchy(skipNavigationName))
                .Concat(Metadata.FindNavigationsInHierarchy(skipNavigationName))
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override InternalEntityTypeBuilder? Ignore(string name, ConfigurationSource configurationSource)
    {
        var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
        if (ignoredConfigurationSource.HasValue)
        {
            if (ignoredConfigurationSource.Value.Overrides(configurationSource))
            {
                return this;
            }
        }
        else if (!CanIgnore(name, configurationSource, shouldThrow: true))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            Metadata.AddIgnored(name, configurationSource);

            var navigation = Metadata.FindNavigation(name);
            if (navigation != null)
            {
                var foreignKey = navigation.ForeignKey;
                Check.DebugAssert(navigation.DeclaringEntityType == Metadata, "navigation.DeclaringEntityType != Metadata");

                if (navigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    ModelBuilder.Metadata.ScopedModelDependencies?.Logger.MappedNavigationIgnoredWarning(navigation);
                }

                var navigationConfigurationSource = navigation.GetConfigurationSource();
                if ((navigation.IsOnDependent
                        && foreignKey.IsOwnership)
                    || (foreignKey.GetConfigurationSource() != navigationConfigurationSource)
                    && (navigation.IsOnDependent
                        || !foreignKey.IsOwnership))
                {
                    var removedNavigation = foreignKey.Builder.HasNavigation(
                        (MemberInfo?)null, navigation.IsOnDependent, configurationSource);
                    Check.DebugAssert(removedNavigation != null, "removedNavigation is null");
                }
                else if (foreignKey.IsOwnership
                         && configurationSource.Overrides(foreignKey.DeclaringEntityType.GetConfigurationSource()))
                {
                    Metadata.Model.Builder.HasNoEntityType(foreignKey.DeclaringEntityType, configurationSource);
                }
                else
                {
                    var removedForeignKey = foreignKey.DeclaringEntityType.Builder.HasNoRelationship(
                        foreignKey, configurationSource);
                    Check.DebugAssert(removedForeignKey != null, "removedForeignKey is null");
                }
            }
            else
            {
                var property = Metadata.FindProperty(name);
                if (property != null)
                {
                    Check.DebugAssert(property.DeclaringType == Metadata, "property.DeclaringEntityType != Metadata");

                    if (property.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        ModelBuilder.Metadata.ScopedModelDependencies?.Logger.MappedPropertyIgnoredWarning(property);
                    }

                    var removedProperty = RemoveProperty(property, configurationSource);

                    Check.DebugAssert(removedProperty != null, "removedProperty is null");
                }
                else
                {
                    var complexProperty = Metadata.FindComplexProperty(name);
                    if (complexProperty != null)
                    {
                        Check.DebugAssert(complexProperty.DeclaringType == Metadata, "property.DeclaringType != Metadata");

                        if (complexProperty.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            ModelBuilder.Metadata.ScopedModelDependencies?.Logger.MappedComplexPropertyIgnoredWarning(complexProperty);
                        }

                        var removedComplexProperty = Metadata.RemoveComplexProperty(complexProperty);

                        Check.DebugAssert(removedComplexProperty != null, "removedProperty is null");
                    }
                    else
                    {
                        var skipNavigation = Metadata.FindSkipNavigation(name);
                        if (skipNavigation != null)
                        {
                            var inverse = skipNavigation.Inverse;
                            if (inverse?.IsInModel == true
                                && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                            {
                                inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource);
                            }

                            Check.DebugAssert(
                                skipNavigation.DeclaringEntityType == Metadata, "skipNavigation.DeclaringEntityType != Metadata");

                            if (skipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                            {
                                ModelBuilder.Metadata.ScopedModelDependencies?.Logger.MappedNavigationIgnoredWarning(skipNavigation);
                            }

                            Metadata.Builder.HasNoSkipNavigation(skipNavigation, configurationSource);
                        }
                        else
                        {
                            var serviceProperty = Metadata.FindServiceProperty(name);
                            if (serviceProperty != null)
                            {
                                Check.DebugAssert(
                                    serviceProperty.DeclaringEntityType == Metadata, "serviceProperty.DeclaringEntityType != Metadata");

                                Metadata.RemoveServiceProperty(serviceProperty);
                            }
                        }
                    }
                }
            }

            foreach (EntityType derivedType in Metadata.GetDerivedTypes())
            {
                var derivedIgnoredSource = derivedType.FindDeclaredIgnoredConfigurationSource(name);
                if (derivedIgnoredSource.HasValue)
                {
                    if (configurationSource.Overrides(derivedIgnoredSource))
                    {
                        derivedType.RemoveIgnored(name);
                    }

                    continue;
                }

                var derivedNavigation = derivedType.FindDeclaredNavigation(name);
                if (derivedNavigation != null)
                {
                    var foreignKey = derivedNavigation.ForeignKey;
                    if (foreignKey.GetConfigurationSource() != derivedNavigation.GetConfigurationSource()
                        && (derivedNavigation.IsOnDependent
                            || !foreignKey.IsOwnership))
                    {
                        if (derivedNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            foreignKey.Builder.HasNavigation(
                                (MemberInfo?)null, derivedNavigation.IsOnDependent, configurationSource);
                        }
                    }
                    else if (foreignKey.IsOwnership
                             && configurationSource.Overrides(foreignKey.DeclaringEntityType.GetConfigurationSource()))
                    {
                        Metadata.Model.Builder.HasNoEntityType(foreignKey.DeclaringEntityType, configurationSource);
                    }
                    else if (foreignKey.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        foreignKey.DeclaringEntityType.Builder.HasNoRelationship(
                            foreignKey, configurationSource);
                    }
                }
                else
                {
                    var derivedProperty = derivedType.FindDeclaredProperty(name);
                    if (derivedProperty != null)
                    {
                        derivedType.Builder.RemoveProperty(
                            derivedProperty, configurationSource,
                            canOverrideSameSource: configurationSource != ConfigurationSource.Explicit);
                    }
                    else
                    {
                        var declaredComplexProperty = derivedType.FindDeclaredComplexProperty(name);
                        if (declaredComplexProperty != null)
                        {
                            if (configurationSource.Overrides(declaredComplexProperty.GetConfigurationSource())
                                && declaredComplexProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                            {
                                derivedType.RemoveComplexProperty(declaredComplexProperty);
                            }
                        }
                        else
                        {
                            var skipNavigation = derivedType.FindDeclaredSkipNavigation(name);
                            if (skipNavigation != null)
                            {
                                var inverse = skipNavigation.Inverse;
                                if (inverse?.IsInModel == true
                                    && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource);
                                }

                                if (skipNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    derivedType.Builder.HasNoSkipNavigation(skipNavigation, configurationSource);
                                }
                            }
                            else
                            {
                                var derivedServiceProperty = derivedType.FindDeclaredServiceProperty(name);
                                if (derivedServiceProperty != null
                                    && configurationSource.Overrides(derivedServiceProperty.GetConfigurationSource())
                                    && derivedServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    derivedType.RemoveServiceProperty(name);
                                }
                            }
                        }
                    }
                }
            }
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool CanIgnore(string name, ConfigurationSource configurationSource, bool shouldThrow)
    {
        var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
        if (ignoredConfigurationSource.HasValue)
        {
            return true;
        }

        var navigation = Metadata.FindNavigation(name);
        if (navigation != null)
        {
            if (navigation.DeclaringEntityType != Metadata)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.InheritedPropertyCannotBeIgnored(
                            name, Metadata.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                }

                return false;
            }

            if (!configurationSource.Overrides(navigation.GetConfigurationSource()))
            {
                return false;
            }
        }
        else
        {
            var property = Metadata.FindProperty(name);
            if (property != null)
            {
                if (property.DeclaringType != Metadata)
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                name, Metadata.DisplayName(), property.DeclaringType.DisplayName()));
                    }

                    return false;
                }

                if (!property.DeclaringType.Builder.CanRemoveProperty(
                        property, configurationSource, canOverrideSameSource: true))
                {
                    return false;
                }
            }
            else
            {
                var complexProperty = Metadata.FindComplexProperty(name);
                if (complexProperty != null)
                {
                    if (complexProperty.DeclaringType != Metadata)
                    {
                        if (shouldThrow)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    name, Metadata.DisplayName(), complexProperty.DeclaringType.DisplayName()));
                        }

                        return false;
                    }

                    if (!configurationSource.Overrides(complexProperty.GetConfigurationSource()))
                    {
                        return false;
                    }
                }
                else
                {
                    var skipNavigation = Metadata.FindSkipNavigation(name);
                    if (skipNavigation != null)
                    {
                        if (skipNavigation.DeclaringEntityType != Metadata)
                        {
                            if (shouldThrow)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.InheritedPropertyCannotBeIgnored(
                                        name, Metadata.DisplayName(), skipNavigation.DeclaringEntityType.DisplayName()));
                            }

                            return false;
                        }

                        if (!configurationSource.Overrides(skipNavigation.GetConfigurationSource()))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var serviceProperty = Metadata.FindServiceProperty(name);
                        if (serviceProperty != null)
                        {
                            if (serviceProperty.DeclaringEntityType != Metadata)
                            {
                                if (shouldThrow)
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.InheritedPropertyCannotBeIgnored(
                                            name, Metadata.DisplayName(), serviceProperty.DeclaringEntityType.DisplayName()));
                                }

                                return false;
                            }

                            if (!configurationSource.Overrides(serviceProperty.GetConfigurationSource()))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTriggerBuilder? HasTrigger(
        string modelName,
        ConfigurationSource configurationSource)
    {
        var entityType = Metadata;
        var trigger = entityType.FindDeclaredTrigger(modelName);
        if (trigger != null)
        {
            trigger.UpdateConfigurationSource(configurationSource);
            return trigger.Builder;
        }

        trigger = entityType.AddTrigger(modelName, configurationSource);

        return trigger?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasQueryFilter(
        LambdaExpression? filter,
        ConfigurationSource configurationSource)
    {
        if (CanSetQueryFilter(filter, configurationSource))
        {
            Metadata.SetQueryFilter(filter, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetQueryFilter(LambdaExpression? filter, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetQueryFilterConfigurationSource())
            || Metadata.GetQueryFilter() == filter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete]
    public virtual InternalEntityTypeBuilder? HasDefiningQuery(
        LambdaExpression? query,
        ConfigurationSource configurationSource)
    {
        if (CanSetDefiningQuery(query, configurationSource))
        {
            Metadata.SetDefiningQuery(query, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete]
    public virtual bool CanSetDefiningQuery(LambdaExpression? query, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetDefiningQueryConfigurationSource())
            || Metadata.GetDefiningQuery() == query;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasBaseType(Type? baseEntityType, ConfigurationSource configurationSource)
    {
        if (baseEntityType == null)
        {
            return HasBaseType((EntityType?)null, configurationSource);
        }

        var baseType = ModelBuilder.Entity(baseEntityType, configurationSource, shouldBeOwned: Metadata.IsOwned());
        return baseType == null
            ? null
            : HasBaseType(baseType.Metadata, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasBaseType(string? baseEntityTypeName, ConfigurationSource configurationSource)
    {
        if (baseEntityTypeName == null)
        {
            return HasBaseType((EntityType?)null, configurationSource);
        }

        var baseType = ModelBuilder.Entity(baseEntityTypeName, configurationSource, shouldBeOwned: Metadata.IsOwned());
        return baseType == null
            ? null
            : HasBaseType(baseType.Metadata, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasBaseType(
        EntityType? baseEntityType,
        ConfigurationSource configurationSource)
    {
        if (Metadata.BaseType == baseEntityType)
        {
            Metadata.SetBaseType(baseEntityType, configurationSource);
            return this;
        }

        if (!CanSetBaseType(baseEntityType, configurationSource))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            List<RelationshipSnapshot>? detachedRelationships = null;
            List<InternalSkipNavigationBuilder>? detachedSkipNavigations = null;
            PropertiesSnapshot? detachedProperties = null;
            List<ComplexPropertySnapshot>? detachedComplexProperties = null;
            List<InternalServicePropertyBuilder>? detachedServiceProperties = null;
            IReadOnlyList<(InternalKeyBuilder, ConfigurationSource?)>? detachedKeys = null;
            // We use at least DataAnnotation as ConfigurationSource while removing to allow us
            // to remove metadata object which were defined in derived type
            // while corresponding annotations were present on properties in base type.
            var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
            if (baseEntityType != null)
            {
                var baseMemberNames = baseEntityType.GetMembers()
                    .ToDictionary(m => m.Name, m => (ConfigurationSource?)m.GetConfigurationSource());

                var relationshipsToBeDetached =
                    FindConflictingMembers(
                            Metadata.GetDerivedTypesInclusive().Cast<EntityType>().SelectMany(et => et.GetDeclaredNavigations()),
                            baseMemberNames,
                            n =>
                            {
                                var baseNavigation = baseEntityType.FindNavigation(n.Name);
                                return baseNavigation != null
                                    && n.TargetEntityType == baseNavigation.TargetEntityType;
                            },
                            n => n.ForeignKey.DeclaringEntityType.Builder.HasNoRelationship(n.ForeignKey, ConfigurationSource.Explicit))
                        ?.Select(n => n.ForeignKey).ToHashSet();

                foreach (var key in Metadata.GetDeclaredKeys().ToList())
                {
                    foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                    {
                        var navigationToDependent = referencingForeignKey.PrincipalToDependent;
                        if (navigationToDependent != null
                            && baseMemberNames.TryGetValue(navigationToDependent.Name, out var baseConfigurationSource)
                            && baseConfigurationSource == ConfigurationSource.Explicit
                            && configurationSource == ConfigurationSource.Explicit
                            && navigationToDependent.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            IReadOnlyPropertyBase baseProperty =
                                baseEntityType.FindMembersInHierarchy(navigationToDependent.Name).Single();
                            if (baseProperty is not IReadOnlyNavigation)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.DuplicatePropertiesOnBase(
                                        Metadata.DisplayName(),
                                        baseEntityType.DisplayName(),
                                        navigationToDependent.DeclaringEntityType.DisplayName(),
                                        navigationToDependent.Name,
                                        baseProperty.DeclaringType.DisplayName(),
                                        baseProperty.Name));
                            }
                        }

                        relationshipsToBeDetached ??= [];

                        relationshipsToBeDetached.Add(referencingForeignKey);
                    }
                }

                if (relationshipsToBeDetached != null)
                {
                    detachedRelationships = [];
                    foreach (var relationshipToBeDetached in relationshipsToBeDetached)
                    {
                        detachedRelationships.Add(DetachRelationship(relationshipToBeDetached));
                    }
                }

                var foreignKeysUsingKeyProperties = Metadata.GetDerivedTypesInclusive().Cast<EntityType>()
                    .SelectMany(t => t.GetDeclaredForeignKeys())
                    .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true));

                foreach (var foreignKeyUsingKeyProperties in foreignKeysUsingKeyProperties.ToList())
                {
                    foreignKeyUsingKeyProperties.Builder.HasForeignKey((IReadOnlyList<Property>?)null, configurationSourceForRemoval);
                }

                var skipNavigationsToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().Cast<EntityType>().SelectMany(et => et.GetDeclaredSkipNavigations()),
                        baseMemberNames,
                        n =>
                        {
                            var baseNavigation = baseEntityType.FindSkipNavigation(n.Name);
                            return baseNavigation != null
                                && n.TargetEntityType == baseNavigation.TargetEntityType;
                        },
                        n => n.DeclaringEntityType.Builder.HasNoSkipNavigation(n, ConfigurationSource.Explicit));

                if (skipNavigationsToDetach != null)
                {
                    detachedSkipNavigations = [];
                    foreach (var skipNavigation in skipNavigationsToDetach)
                    {
                        detachedSkipNavigations.Add(DetachSkipNavigation(skipNavigation)!);
                    }
                }

                detachedKeys = DetachKeys(Metadata.GetDeclaredKeys());

                Metadata.SetIsKeyless(false, configurationSource);

                var propertiesToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties()),
                        baseMemberNames,
                        p => baseEntityType.FindProperty(p.Name) != null,
                        p => p.DeclaringType.Builder.RemoveProperty(p, ConfigurationSource.Explicit));

                if (propertiesToDetach != null)
                {
                    detachedProperties = DetachProperties(propertiesToDetach);
                }

                var complexPropertiesToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredComplexProperties()),
                        baseMemberNames,
                        p => baseEntityType.FindComplexProperty(p.Name) != null,
                        p => p.DeclaringType.RemoveComplexProperty(p));

                if (complexPropertiesToDetach != null)
                {
                    detachedComplexProperties = [];
                    foreach (var complexPropertyToDetach in complexPropertiesToDetach)
                    {
                        detachedComplexProperties.Add(InternalComplexPropertyBuilder.Detach(complexPropertyToDetach)!);
                    }
                }

                var servicePropertiesToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().Cast<EntityType>().SelectMany(et => et.GetDeclaredServiceProperties()),
                        baseMemberNames,
                        n => baseEntityType.FindServiceProperty(n.Name) != null,
                        p => p.DeclaringEntityType.RemoveServiceProperty(p));

                if (servicePropertiesToDetach != null)
                {
                    detachedServiceProperties = [];
                    foreach (var serviceProperty in servicePropertiesToDetach)
                    {
                        detachedServiceProperties.Add(DetachServiceProperty(serviceProperty)!);
                    }
                }

                foreach (var ignoredMember in Metadata.GetIgnoredMembers().ToList())
                {
                    if (baseEntityType.FindIgnoredConfigurationSource(ignoredMember)
                        .Overrides(Metadata.FindDeclaredIgnoredConfigurationSource(ignoredMember)))
                    {
                        Metadata.RemoveIgnored(ignoredMember);
                    }
                }

                baseEntityType.UpdateConfigurationSource(configurationSource);
            }

            List<InternalIndexBuilder>? detachedIndexes = null;
            HashSet<Property>? removedInheritedPropertiesToDuplicate = null;
            if (Metadata.BaseType != null)
            {
                var removedInheritedProperties = new HashSet<Property>(
                    Metadata.BaseType.GetProperties()
                        .Where(p => baseEntityType == null || baseEntityType.FindProperty(p.Name) != p));
                if (removedInheritedProperties.Count != 0)
                {
                    removedInheritedPropertiesToDuplicate = [];
                    List<ForeignKey>? relationshipsToBeDetached = null;
                    foreach (var foreignKey in Metadata.GetDerivedTypesInclusive().Cast<EntityType>()
                                 .SelectMany(t => t.GetDeclaredForeignKeys()))
                    {
                        var shouldBeDetached = false;
                        foreach (var property in foreignKey.Properties)
                        {
                            if (removedInheritedProperties.Contains(property))
                            {
                                removedInheritedPropertiesToDuplicate.Add(property);
                                shouldBeDetached = true;
                            }
                        }

                        if (!shouldBeDetached)
                        {
                            continue;
                        }

                        relationshipsToBeDetached ??= [];

                        relationshipsToBeDetached.Add(foreignKey);
                    }

                    foreach (var key in Metadata.GetKeys())
                    {
                        if (key.ReferencingForeignKeys == null
                            || !key.ReferencingForeignKeys.Any()
                            || !key.Properties.Any(p => removedInheritedProperties.Contains(p)))
                        {
                            continue;
                        }

                        foreach (var referencingForeignKey in key.ReferencingForeignKeys.ToList())
                        {
                            if (Metadata.IsAssignableFrom(referencingForeignKey.PrincipalEntityType))
                            {
                                relationshipsToBeDetached ??= [];

                                relationshipsToBeDetached.Add(referencingForeignKey);
                            }
                        }
                    }

                    if (relationshipsToBeDetached != null)
                    {
                        detachedRelationships = [];
                        foreach (var relationshipToBeDetached in relationshipsToBeDetached)
                        {
                            detachedRelationships.Add(DetachRelationship(relationshipToBeDetached));
                        }
                    }

                    List<Index>? indexesToBeDetached = null;
                    foreach (var index in Metadata.GetDerivedTypesInclusive().Cast<EntityType>().SelectMany(e => e.GetDeclaredIndexes()))
                    {
                        var shouldBeDetached = false;
                        foreach (var property in index.Properties)
                        {
                            if (removedInheritedProperties.Contains(property))
                            {
                                removedInheritedPropertiesToDuplicate.Add(property);
                                shouldBeDetached = true;
                            }
                        }

                        if (!shouldBeDetached)
                        {
                            continue;
                        }

                        indexesToBeDetached ??= [];

                        indexesToBeDetached.Add(index);
                    }

                    if (indexesToBeDetached != null)
                    {
                        detachedIndexes = [];
                        foreach (var indexToBeDetached in indexesToBeDetached)
                        {
                            detachedIndexes.Add(DetachIndex(indexToBeDetached));
                        }
                    }
                }
            }

            Metadata.SetBaseType(baseEntityType, configurationSource);

            if (removedInheritedPropertiesToDuplicate != null)
            {
                foreach (var property in removedInheritedPropertiesToDuplicate)
                {
                    if (property.IsInModel)
                    {
                        property.Builder.Attach(this);
                    }
                }
            }

            if (detachedServiceProperties != null)
            {
                foreach (var detachedServiceProperty in detachedServiceProperties)
                {
                    detachedServiceProperty.Attach(detachedServiceProperty.Metadata.DeclaringEntityType.Builder);
                }
            }

            if (detachedComplexProperties != null)
            {
                foreach (var detachedComplexProperty in detachedComplexProperties)
                {
                    detachedComplexProperty.Attach(
                        detachedComplexProperty.ComplexProperty.DeclaringType.Builder);
                }
            }

            detachedProperties?.Attach(this);

            if (detachedKeys != null)
            {
                foreach (var (internalKeyBuilder, value) in detachedKeys)
                {
                    var newKeyBuilder = internalKeyBuilder.Attach(Metadata.GetRootType().Builder, value);
                    if (newKeyBuilder == null
                        && internalKeyBuilder.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(Metadata.DisplayName()));
                    }
                }
            }

            if (detachedIndexes != null)
            {
                foreach (var detachedIndex in detachedIndexes)
                {
                    detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
                }
            }

            if (detachedSkipNavigations != null)
            {
                foreach (var detachedSkipNavigation in detachedSkipNavigations)
                {
                    detachedSkipNavigation.Attach();
                }
            }

            if (detachedRelationships != null)
            {
                foreach (var detachedRelationship in detachedRelationships)
                {
                    detachedRelationship.Attach();
                }
            }
        }

        return this;

        List<T>? FindConflictingMembers<T>(
            IEnumerable<T> derivedMembers,
            Dictionary<string, ConfigurationSource?> baseMemberNames,
            Func<T, bool> compatibleWithBaseMember,
            Action<T> removeMember)
            where T : PropertyBase
        {
            List<T>? membersToBeDetached = null;
            List<T>? membersToBeRemoved = null;
            foreach (var member in derivedMembers)
            {
                ConfigurationSource? baseConfigurationSource = null;
                if ((!member.GetConfigurationSource().OverridesStrictly(
                            baseEntityType.FindIgnoredConfigurationSource(member.Name))
                        && member.GetConfigurationSource() != ConfigurationSource.Explicit)
                    || (baseMemberNames.TryGetValue(member.Name, out baseConfigurationSource)
                        && baseConfigurationSource.Overrides(member.GetConfigurationSource())
                        && !compatibleWithBaseMember(member)))
                {
                    if (baseConfigurationSource == ConfigurationSource.Explicit
                        && configurationSource == ConfigurationSource.Explicit
                        && member.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.DuplicatePropertiesOnBase(
                                Metadata.DisplayName(),
                                baseEntityType.DisplayName(),
                                ((IReadOnlyTypeBase)member.DeclaringType).DisplayName(),
                                member.Name,
                                baseEntityType.DisplayName(),
                                member.Name));
                    }

                    membersToBeRemoved ??= [];

                    membersToBeRemoved.Add(member);
                    continue;
                }

                if (baseConfigurationSource != null)
                {
                    membersToBeDetached ??= [];

                    membersToBeDetached.Add(member);
                }
            }

            if (membersToBeRemoved != null)
            {
                foreach (var memberToBeRemoved in membersToBeRemoved)
                {
                    removeMember(memberToBeRemoved);
                }
            }

            return membersToBeDetached;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetBaseType(EntityType? baseEntityType, ConfigurationSource configurationSource)
    {
        if (Metadata.BaseType == baseEntityType
            || configurationSource == ConfigurationSource.Explicit)
        {
            return true;
        }

        if (!configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource()))
        {
            return false;
        }

        if (baseEntityType == null)
        {
            return true;
        }

        var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
        if (Metadata.GetDeclaredKeys().Any(
                k => !configurationSourceForRemoval.Overrides(k.GetConfigurationSource())
                    && k.Properties.Any(p => baseEntityType.FindProperty(p.Name) == null))
            || (Metadata.IsKeyless && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource())))
        {
            return false;
        }

        if (Metadata.GetDerivedTypesInclusive().Cast<EntityType>()
            .SelectMany(t => t.GetDeclaredForeignKeys())
            .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true))
            .Any(fk => !configurationSourceForRemoval.Overrides(fk.GetPropertiesConfigurationSource())))
        {
            return false;
        }

        var baseMembers = baseEntityType.GetMembers()
            .Where(m => m.GetConfigurationSource() == ConfigurationSource.Explicit)
            .ToDictionary(m => m.Name);

        foreach (var derivedMember in Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredMembers()))
        {
            if (derivedMember.GetConfigurationSource() == ConfigurationSource.Explicit
                && baseMembers.TryGetValue(derivedMember.Name, out var baseMember))
            {
                switch (derivedMember)
                {
                    case IReadOnlyProperty:
                        return baseMember is IReadOnlyProperty;
                    case IReadOnlyNavigation derivedNavigation:
                        return baseMember is IReadOnlyNavigation baseNavigation
                            && derivedNavigation.TargetEntityType == baseNavigation.TargetEntityType;
                    case IReadOnlyComplexProperty:
                        return baseMember is IReadOnlyComplexProperty;
                    case IReadOnlyServiceProperty:
                        return baseMember is IReadOnlyServiceProperty;
                    case IReadOnlySkipNavigation derivedSkipNavigation:
                        return baseMember is IReadOnlySkipNavigation baseSkipNavigation
                            && derivedSkipNavigation.TargetEntityType == baseSkipNavigation.TargetEntityType;
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveForeignKey(ForeignKey foreignKey, ConfigurationSource configurationSource)
    {
        Check.DebugAssert(foreignKey.DeclaringEntityType == Metadata, "foreignKey.DeclaringEntityType != Metadata");

        return configurationSource.Overrides(foreignKey.GetConfigurationSource());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveSkipNavigation(SkipNavigation skipNavigation, ConfigurationSource? configurationSource)
    {
        Check.DebugAssert(skipNavigation.DeclaringEntityType == Metadata, "skipNavigation.DeclaringEntityType != Metadata");

        return configurationSource.Overrides(skipNavigation.GetConfigurationSource());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationshipSnapshot DetachRelationship(ForeignKey foreignKey)
        => DetachRelationship(foreignKey, false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RelationshipSnapshot DetachRelationship(ForeignKey foreignKey, bool includeOwnedSharedType)
    {
        var detachedBuilder = foreignKey.Builder;
        var referencingSkipNavigations = foreignKey.ReferencingSkipNavigations?
            .Select(s => (s, s.GetForeignKeyConfigurationSource()!.Value)).ToList();
        var relationshipConfigurationSource = foreignKey.DeclaringEntityType.Builder
            .HasNoRelationship(foreignKey, foreignKey.GetConfigurationSource());
        Check.DebugAssert(relationshipConfigurationSource != null, "relationshipConfigurationSource is null");

        EntityType.Snapshot? ownedSnapshot = null;
        var dependentEntityType = foreignKey.DeclaringEntityType;
        if (includeOwnedSharedType
            && foreignKey.IsOwnership
            && dependentEntityType.HasSharedClrType)
        {
            ownedSnapshot = DetachAllMembers(dependentEntityType);
            dependentEntityType.Model.Builder.HasNoEntityType(dependentEntityType, ConfigurationSource.Explicit);
        }

        return new RelationshipSnapshot(detachedBuilder, ownedSnapshot, referencingSkipNavigations);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoRelationship(
        ForeignKey foreignKey,
        ConfigurationSource configurationSource)
    {
        if (!foreignKey.IsInModel)
        {
            return this;
        }

        var currentConfigurationSource = foreignKey.GetConfigurationSource();
        if (!configurationSource.Overrides(currentConfigurationSource))
        {
            return null;
        }

        if (foreignKey.ReferencingSkipNavigations != null)
        {
            foreach (var referencingSkipNavigation in foreignKey.ReferencingSkipNavigations.ToList())
            {
                Check.DebugAssert(
                    currentConfigurationSource.Overrides(referencingSkipNavigation.GetForeignKeyConfigurationSource()),
                    "Setting the FK on the skip navigation should upgrade the configuration source");

                referencingSkipNavigation.Builder.HasForeignKey(null, configurationSource);
            }
        }

        if (!foreignKey.IsInModel)
        {
            return this;
        }

        Metadata.RemoveForeignKey(foreignKey);

        RemoveUnusedImplicitProperties(foreignKey.Properties);
        if (foreignKey.PrincipalKey.DeclaringEntityType.IsInModel)
        {
            foreignKey.PrincipalKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(foreignKey.PrincipalKey);
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static EntityType.Snapshot? DetachAllMembers(EntityType entityType)
    {
        if (!entityType.IsInModel)
        {
            return null;
        }

        List<RelationshipSnapshot>? detachedRelationships = null;
        foreach (var relationshipToBeDetached in entityType.GetDeclaredForeignKeys().ToList())
        {
            detachedRelationships ??= [];

            var detachedRelationship = DetachRelationship(relationshipToBeDetached, false);
            if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                || relationshipToBeDetached.IsOwnership)
            {
                detachedRelationships.Add(detachedRelationship);
            }
        }

        List<InternalSkipNavigationBuilder>? detachedSkipNavigations = null;
        foreach (var skipNavigationsToBeDetached in entityType.GetDeclaredSkipNavigations().ToList())
        {
            detachedSkipNavigations ??= [];

            detachedSkipNavigations.Add(DetachSkipNavigation(skipNavigationsToBeDetached)!);
        }

        List<(InternalKeyBuilder, ConfigurationSource?)>? detachedKeys = null;
        foreach (var keyToDetach in entityType.GetDeclaredKeys().ToList())
        {
            foreach (var relationshipToBeDetached in keyToDetach.GetReferencingForeignKeys().ToList())
            {
                if (!relationshipToBeDetached.IsInModel
                    || !relationshipToBeDetached.DeclaringEntityType.IsInModel)
                {
                    // Referencing type might have been removed while removing other foreign keys
                    continue;
                }

                detachedRelationships ??= [];

                var detachedRelationship = DetachRelationship(relationshipToBeDetached, true);
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

            var detachedKey = DetachKey(keyToDetach);
            if (detachedKey.Item1.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
            {
                detachedKeys.Add(detachedKey);
            }
        }

        List<InternalIndexBuilder>? detachedIndexes = null;
        foreach (var indexToBeDetached in entityType.GetDeclaredIndexes().ToList())
        {
            detachedIndexes ??= [];

            var detachedIndex = DetachIndex(indexToBeDetached);
            if (detachedIndex.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
            {
                detachedIndexes.Add(detachedIndex);
            }
        }

        var detachedProperties = DetachProperties(entityType.GetDeclaredProperties().ToList());

        List<InternalServicePropertyBuilder>? detachedServiceProperties = null;
        foreach (var servicePropertiesToBeDetached in entityType.GetDeclaredServiceProperties().ToList())
        {
            detachedServiceProperties ??= [];

            detachedServiceProperties.Add(DetachServiceProperty(servicePropertiesToBeDetached)!);
        }

        return new EntityType.Snapshot(
            entityType,
            detachedProperties,
            detachedIndexes,
            detachedKeys,
            detachedRelationships,
            detachedSkipNavigations,
            detachedServiceProperties);
    }

    private void RemoveKeyIfUnused(Key key, ConfigurationSource configurationSource = ConfigurationSource.Convention)
    {
        if (Metadata.FindPrimaryKey() == key
            || key.ReferencingForeignKeys?.Any() == true)
        {
            return;
        }

        HasNoKey(key, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(
        IReadOnlyList<string> propertyNames,
        string name,
        ConfigurationSource configurationSource)
        => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(
        IReadOnlyList<MemberInfo> clrMembers,
        ConfigurationSource configurationSource)
        => HasIndex(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(
        IReadOnlyList<MemberInfo> clrMembers,
        string name,
        ConfigurationSource configurationSource)
        => HasIndex(GetOrCreateProperties(clrMembers, configurationSource), name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(
        IReadOnlyList<Property>? properties,
        ConfigurationSource configurationSource)
    {
        if (properties == null)
        {
            return null;
        }

        List<InternalIndexBuilder>? detachedIndexes = null;
        var existingIndex = Metadata.FindIndex(properties);
        if (existingIndex == null)
        {
            detachedIndexes = Metadata.FindDerivedIndexes(properties).ToList().Select(DetachIndex).ToList();
        }
        else if (existingIndex.DeclaringEntityType != Metadata)
        {
            return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, null, configurationSource);
        }

        var indexBuilder = HasIndex(existingIndex, properties, null, configurationSource);

        if (detachedIndexes != null)
        {
            foreach (var detachedIndex in detachedIndexes)
            {
                detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
            }
        }

        return indexBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalIndexBuilder? HasIndex(
        IReadOnlyList<Property>? properties,
        string name,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(name, nameof(name));

        if (properties == null)
        {
            return null;
        }

        List<InternalIndexBuilder>? detachedIndexes = null;

        var existingIndex = Metadata.FindIndex(name);
        if (existingIndex != null
            && !existingIndex.Properties.SequenceEqual(properties))
        {
            // use existing index only if properties match
            existingIndex = null;
        }

        if (existingIndex == null)
        {
            detachedIndexes = Metadata.FindDerivedIndexes(name)
                .Where(i => i.Properties.SequenceEqual(properties))
                .ToList().Select(DetachIndex).ToList();
        }
        else if (existingIndex.DeclaringEntityType != Metadata)
        {
            return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, name, configurationSource);
        }

        var indexBuilder = HasIndex(existingIndex, properties, name, configurationSource);

        if (detachedIndexes != null)
        {
            foreach (var detachedIndex in detachedIndexes)
            {
                detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
            }
        }

        return indexBuilder;
    }

    private InternalIndexBuilder? HasIndex(
        Index? index,
        IReadOnlyList<Property> properties,
        string? name,
        ConfigurationSource configurationSource)
    {
        if (index == null)
        {
            index = name == null
                ? Metadata.AddIndex(properties, configurationSource)
                : Metadata.AddIndex(properties, name, configurationSource);
        }
        else
        {
            index.UpdateConfigurationSource(configurationSource);
        }

        return index?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveIndex(
        IReadOnlyList<string> propertyNames,
        ConfigurationSource configurationSource)
    {
        for (var i = 0; i < propertyNames.Count; i++)
        {
            if (!CanHaveProperty(
                    propertyType: null,
                    propertyNames[i],
                    null,
                    typeConfigurationSource: null,
                    configurationSource,
                    checkClrProperty: true))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoIndex(Index index, ConfigurationSource configurationSource)
    {
        var currentConfigurationSource = index.GetConfigurationSource();
        if (!configurationSource.Overrides(currentConfigurationSource))
        {
            return null;
        }

        var removedIndex = index.Name == null
            ? Metadata.RemoveIndex(index.Properties)
            : Metadata.RemoveIndex(index.Name);
        Check.DebugAssert(removedIndex == index, "removedIndex != index");

        RemoveUnusedImplicitProperties(index.Properties);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveIndex(Index index, ConfigurationSource configurationSource)
        => configurationSource.Overrides(index.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static List<InternalIndexBuilder>? DetachIndexes(IEnumerable<Index> indexesToDetach)
    {
        var indexesToDetachList = (indexesToDetach as List<Index>) ?? indexesToDetach.ToList();
        if (indexesToDetachList.Count == 0)
        {
            return null;
        }

        var detachedIndexes = new List<InternalIndexBuilder>();
        foreach (var indexToDetach in indexesToDetachList)
        {
            var detachedIndex = DetachIndex(indexToDetach);
            detachedIndexes.Add(detachedIndex);
        }

        return detachedIndexes;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static InternalIndexBuilder DetachIndex(Index indexToDetach)
    {
        var entityTypeBuilder = indexToDetach.DeclaringEntityType.Builder;
        var indexBuilder = indexToDetach.Builder;
        var removedConfigurationSource = entityTypeBuilder.HasNoIndex(indexToDetach, indexToDetach.GetConfigurationSource());
        Check.DebugAssert(removedConfigurationSource != null, "removedConfigurationSource is null");
        return indexBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        string principalEntityTypeName,
        IReadOnlyList<string> propertyNames,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
        Check.NotEmpty(propertyNames, nameof(propertyNames));

        var principalTypeBuilder = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
        var principalKey = principalTypeBuilder?.Metadata.FindPrimaryKey();
        return principalTypeBuilder == null
            ? null
            : HasForeignKey(
                principalTypeBuilder.Metadata,
                GetOrCreateProperties(
                    propertyNames, configurationSource, principalKey?.Properties, useDefaultType: principalKey == null),
                null,
                configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        string principalEntityTypeName,
        IReadOnlyList<string> propertyNames,
        Key principalKey,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
        Check.NotEmpty(propertyNames, nameof(propertyNames));

        var principalTypeBuilder = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
        return principalTypeBuilder == null
            ? null
            : HasForeignKey(
                principalTypeBuilder.Metadata,
                GetOrCreateProperties(propertyNames, configurationSource, principalKey.Properties),
                principalKey,
                configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        Type principalClrType,
        IReadOnlyList<MemberInfo> clrMembers,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(principalClrType, nameof(principalClrType));
        Check.NotEmpty(clrMembers, nameof(clrMembers));

        var principalTypeBuilder = ModelBuilder.Entity(
            principalClrType, configurationSource, shouldBeOwned: Metadata.IsInOwnershipPath(principalClrType) ? null : false);
        return principalTypeBuilder == null
            ? null
            : HasForeignKey(
                principalTypeBuilder.Metadata,
                GetOrCreateProperties(clrMembers, configurationSource),
                null,
                configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        Type principalClrType,
        IReadOnlyList<MemberInfo> clrMembers,
        Key principalKey,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(principalClrType, nameof(principalClrType));
        Check.NotEmpty(clrMembers, nameof(clrMembers));

        var principalTypeBuilder = ModelBuilder.Entity(
            principalClrType, configurationSource, shouldBeOwned: Metadata.IsInOwnershipPath(principalClrType) ? null : false);
        return principalTypeBuilder == null
            ? null
            : HasForeignKey(
                principalTypeBuilder.Metadata,
                GetOrCreateProperties(clrMembers, configurationSource),
                principalKey,
                configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType principalEntityType,
        IReadOnlyList<Property> dependentProperties,
        ConfigurationSource configurationSource)
        => HasForeignKey(
            principalEntityType,
            GetActualProperties(dependentProperties, configurationSource),
            null,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType principalEntityType,
        IReadOnlyList<Property> dependentProperties,
        Key? principalKey,
        ConfigurationSource configurationSource)
        => HasForeignKey(
            principalEntityType,
            GetActualProperties(dependentProperties, configurationSource),
            principalKey,
            configurationSource);

    private InternalForeignKeyBuilder? HasForeignKey(
        EntityType principalEntityType,
        IReadOnlyList<Property>? dependentProperties,
        Key? principalKey,
        ConfigurationSource configurationSource)
    {
        if (dependentProperties == null)
        {
            return null;
        }

        var newRelationship = HasRelationshipInternal(principalEntityType, principalKey, configurationSource)!;

        var relationship = newRelationship.HasForeignKey(dependentProperties, configurationSource);
        if (relationship == null
            && newRelationship.Metadata.IsInModel)
        {
            HasNoRelationship(newRelationship.Metadata, configurationSource);
        }

        newRelationship = relationship;

        return newRelationship;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType targetEntityType,
        string? navigationName,
        ConfigurationSource configurationSource,
        bool? targetIsPrincipal = null)
        => HasRelationship(
            Check.NotNull(targetEntityType, nameof(targetEntityType)),
            MemberIdentity.Create(navigationName),
            null,
            targetIsPrincipal,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType targetEntityType,
        MemberInfo? navigationMember,
        ConfigurationSource configurationSource,
        bool? targetIsPrincipal = null)
        => HasRelationship(
            Check.NotNull(targetEntityType, nameof(targetEntityType)),
            MemberIdentity.Create(navigationMember),
            null,
            targetIsPrincipal,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType targetEntityType,
        string? navigationName,
        string? inverseNavigationName,
        ConfigurationSource configurationSource,
        bool setTargetAsPrincipal = false)
        => HasRelationship(
            Check.NotNull(targetEntityType, nameof(targetEntityType)),
            MemberIdentity.Create(navigationName),
            MemberIdentity.Create(inverseNavigationName),
            setTargetAsPrincipal ? true : null,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType targetEntityType,
        MemberInfo? navigation,
        MemberInfo? inverseNavigation,
        ConfigurationSource configurationSource,
        bool setTargetAsPrincipal = false)
        => HasRelationship(
            Check.NotNull(targetEntityType, nameof(targetEntityType)),
            MemberIdentity.Create(navigation),
            MemberIdentity.Create(inverseNavigation),
            setTargetAsPrincipal ? true : null,
            configurationSource);

    private InternalForeignKeyBuilder? HasRelationship(
        EntityType targetEntityType,
        MemberIdentity? navigationToTarget,
        MemberIdentity? inverseNavigation,
        bool? setTargetAsPrincipal,
        ConfigurationSource configurationSource,
        bool? required = null)
    {
        Check.DebugAssert(
            navigationToTarget != null || inverseNavigation != null,
            "navigationToTarget == null and inverseNavigation == null");

        Check.DebugAssert(
            setTargetAsPrincipal != null || required == null,
            "required should only be set if principal end is known");

        var navigationProperty = navigationToTarget?.MemberInfo;
        if (navigationProperty == null
            && navigationToTarget?.Name != null
            && !Metadata.IsPropertyBag)
        {
            navigationProperty = InternalForeignKeyBuilder.FindCompatibleClrMember(
                navigationToTarget.Value.Name!, Metadata, targetEntityType,
                shouldThrow: configurationSource == ConfigurationSource.Explicit);
            if (navigationProperty != null)
            {
                navigationToTarget = MemberIdentity.Create(navigationProperty);
            }
        }

        var inverseProperty = inverseNavigation?.MemberInfo;
        if (setTargetAsPrincipal == false
            || (setTargetAsPrincipal == null
                && inverseNavigation?.Name == null
                && navigationProperty?.GetMemberType().IsAssignableFrom(
                    targetEntityType.ClrType)
                == false))
        {
            // Target is dependent or only one nav specified and it can't be the nav to principal
            return targetEntityType.Builder.HasRelationship(
                Metadata, inverseNavigation, navigationToTarget, setTargetAsPrincipal: true, configurationSource, required);
        }

        if (setTargetAsPrincipal == null
            && targetEntityType.IsKeyless)
        {
            setTargetAsPrincipal = false;
        }

        if (configurationSource == ConfigurationSource.Explicit
            && setTargetAsPrincipal.HasValue)
        {
            if (setTargetAsPrincipal.Value)
            {
                if (targetEntityType.IsKeyless)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrincipalKeylessType(
                            targetEntityType.DisplayName(),
                            targetEntityType.DisplayName()
                            + (inverseNavigation == null
                                ? ""
                                : "." + inverseNavigation.Value.Name),
                            Metadata.DisplayName()
                            + (navigationToTarget == null
                                ? ""
                                : "." + navigationToTarget.Value.Name)));
                }
            }
            else
            {
                if (Metadata.IsKeyless)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrincipalKeylessType(
                            Metadata.DisplayName(),
                            Metadata.DisplayName()
                            + (navigationToTarget == null
                                ? ""
                                : "." + navigationToTarget.Value.Name),
                            targetEntityType.DisplayName()
                            + (inverseNavigation == null
                                ? ""
                                : "." + inverseNavigation.Value.Name)));
                }
            }
        }

        var existingRelationship = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
            targetEntityType,
            Metadata,
            navigationToTarget,
            inverseNavigation,
            dependentProperties: null,
            principalProperties: null);
        if (existingRelationship != null)
        {
            var shouldInvert = false;
            // The dependent and principal sides could be in the same hierarchy so we need to use the navigations to determine
            // the expected principal side.
            // And since both sides are in the same hierarchy different navigations must have different names.
            if (navigationToTarget != null)
            {
                if (navigationToTarget.Value.Name == existingRelationship.Metadata.DependentToPrincipal?.Name)
                {
                    existingRelationship.Metadata.SetDependentToPrincipal(navigationToTarget.Value.Name, configurationSource);
                }
                else if (setTargetAsPrincipal == true)
                {
                    shouldInvert = true;
                }
                else
                {
                    existingRelationship.Metadata.SetPrincipalToDependent(navigationToTarget.Value.Name, configurationSource);
                }

                if (navigationToTarget.Value.Name != null)
                {
                    Metadata.RemoveIgnored(navigationToTarget.Value.Name);
                }
            }

            if (inverseNavigation != null)
            {
                if (inverseNavigation.Value.Name == existingRelationship.Metadata.PrincipalToDependent?.Name)
                {
                    existingRelationship.Metadata.SetPrincipalToDependent(inverseNavigation.Value.Name, configurationSource);
                }
                else if (setTargetAsPrincipal == true)
                {
                    shouldInvert = true;
                }
                else
                {
                    existingRelationship.Metadata.SetDependentToPrincipal(inverseNavigation.Value.Name, configurationSource);
                }

                if (inverseNavigation.Value.Name != null)
                {
                    targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
                }
            }

            existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

            if (!shouldInvert)
            {
                if (setTargetAsPrincipal == true)
                {
                    existingRelationship = existingRelationship.HasEntityTypes(
                        existingRelationship.Metadata.PrincipalEntityType,
                        existingRelationship.Metadata.DeclaringEntityType,
                        configurationSource)!;

                    if (required.HasValue)
                    {
                        existingRelationship = existingRelationship.IsRequired(required.Value, configurationSource);
                    }
                }

                return existingRelationship;
            }

            // If relationship should be inverted it will be handled below
        }
        else
        {
            existingRelationship = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
                Metadata,
                targetEntityType,
                inverseNavigation,
                navigationToTarget,
                dependentProperties: null,
                principalProperties: null);
            if (existingRelationship != null)
            {
                // Since the existing relationship didn't match the first case then the dependent and principal sides
                // are not in the same hierarchy therefore we don't need to check existing navigations
                if (navigationToTarget != null)
                {
                    Check.DebugAssert(
                        navigationToTarget.Value.Name == existingRelationship.Metadata.PrincipalToDependent?.Name,
                        $"Expected {navigationToTarget.Value.Name}, found {existingRelationship.Metadata.PrincipalToDependent?.Name}");

                    existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    if (navigationToTarget.Value.Name != null)
                    {
                        Metadata.RemoveIgnored(navigationToTarget.Value.Name);
                    }
                }

                if (inverseNavigation != null)
                {
                    Check.DebugAssert(
                        inverseNavigation.Value.Name == existingRelationship.Metadata.DependentToPrincipal?.Name,
                        $"Expected {inverseNavigation.Value.Name}, found {existingRelationship.Metadata.DependentToPrincipal?.Name}");

                    existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
                    }
                }

                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

                if (setTargetAsPrincipal == null)
                {
                    return existingRelationship;
                }
            }
        }

        InternalForeignKeyBuilder? relationship;
        InternalForeignKeyBuilder? newRelationship = null;
        using (var batcher = Metadata.Model.DelayConventions())
        {
            if (existingRelationship != null)
            {
                relationship = existingRelationship;
            }
            else
            {
                if (navigationToTarget?.Name != null
                    && navigationToTarget.Value.MemberInfo == null
                    && Metadata.ClrType != Model.DefaultPropertyBagType)
                {
                    navigationProperty = InternalForeignKeyBuilder.FindCompatibleClrMember(
                        navigationToTarget.Value.Name!, Metadata, targetEntityType,
                        shouldThrow: configurationSource == ConfigurationSource.Explicit);
                    if (navigationProperty != null)
                    {
                        navigationToTarget = MemberIdentity.Create(navigationProperty);
                    }
                }

                if (inverseNavigation?.Name != null
                    && inverseNavigation.Value.MemberInfo == null
                    && targetEntityType.ClrType != Model.DefaultPropertyBagType)
                {
                    inverseProperty = InternalForeignKeyBuilder.FindCompatibleClrMember(
                        inverseNavigation.Value.Name!, targetEntityType, Metadata,
                        shouldThrow: configurationSource == ConfigurationSource.Explicit);
                    if (inverseProperty != null)
                    {
                        inverseNavigation = MemberIdentity.Create(inverseProperty);
                    }
                }

                if (!InternalForeignKeyBuilder.AreCompatible(
                        navigationToTarget?.MemberInfo,
                        inverseNavigation?.MemberInfo,
                        targetEntityType,
                        Metadata,
                        shouldThrow: configurationSource == ConfigurationSource.Explicit,
                        out var shouldInvert,
                        out _))
                {
                    return null;
                }

                if (shouldInvert == true && setTargetAsPrincipal == true)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrincipalEndIncompatibleNavigations(
                            Metadata.DisplayName()
                            + (navigationToTarget == null
                                ? ""
                                : "." + navigationToTarget.Value.Name),
                            targetEntityType.DisplayName()
                            + (inverseNavigation == null
                                ? ""
                                : "." + inverseNavigation.Value.Name),
                            targetEntityType.DisplayName()));
                }

                shouldInvert ??= setTargetAsPrincipal != true
                    && (setTargetAsPrincipal != null
                        || targetEntityType.IsInOwnershipPath(Metadata));
                if (!shouldInvert.Value)
                {
                    newRelationship = CreateForeignKey(
                        targetEntityType.Builder,
                        dependentProperties: null,
                        principalKey: null,
                        propertyBaseName: navigationProperty?.GetSimpleMemberName(),
                        required,
                        configurationSource);
                }
                else
                {
                    (navigationToTarget, inverseNavigation) = (inverseNavigation, navigationToTarget);

                    navigationProperty = navigationToTarget?.MemberInfo;
                    inverseProperty = inverseNavigation?.MemberInfo;

                    newRelationship = targetEntityType.Builder.CreateForeignKey(
                        this,
                        dependentProperties: null,
                        principalKey: null,
                        propertyBaseName: navigationProperty?.GetSimpleMemberName(),
                        required: null,
                        configurationSource);
                }

                relationship = newRelationship;
                if (relationship == null)
                {
                    return null;
                }
            }

            if (setTargetAsPrincipal == true)
            {
                relationship = relationship
                    .HasEntityTypes(targetEntityType.Builder.Metadata, Metadata, configurationSource)!;

                if (required.HasValue)
                {
                    relationship = relationship.IsRequired(required.Value, configurationSource)!;
                }
            }

            if (inverseNavigation == null)
            {
                relationship = navigationProperty != null
                    ? relationship.HasNavigation(
                        navigationProperty,
                        pointsToPrincipal: true,
                        configurationSource)
                    : relationship.HasNavigation(
                        navigationToTarget!.Value.Name,
                        pointsToPrincipal: true,
                        configurationSource);
            }
            else if (navigationToTarget == null)
            {
                relationship = inverseProperty != null
                    ? relationship.HasNavigation(
                        inverseProperty,
                        pointsToPrincipal: false,
                        configurationSource)
                    : relationship.HasNavigation(
                        inverseNavigation.Value.Name,
                        pointsToPrincipal: false,
                        configurationSource);
            }
            else
            {
                relationship = navigationProperty != null || inverseProperty != null
                    ? relationship.HasNavigations(navigationProperty, inverseProperty, configurationSource)
                    : relationship.HasNavigations(navigationToTarget.Value.Name, inverseNavigation.Value.Name, configurationSource);
            }

            if (relationship != null)
            {
                relationship = batcher.Run(relationship);
            }
        }

        if (relationship != null
            && ((navigationToTarget != null
                    && relationship.Metadata.DependentToPrincipal?.Name != navigationToTarget.Value.Name)
                || (inverseNavigation != null
                    && relationship.Metadata.PrincipalToDependent?.Name != inverseNavigation.Value.Name))
            && ((inverseNavigation != null
                    && relationship.Metadata.DependentToPrincipal?.Name != inverseNavigation.Value.Name)
                || (navigationToTarget != null
                    && relationship.Metadata.PrincipalToDependent?.Name != navigationToTarget.Value.Name)))
        {
            relationship = null;
        }

        if (relationship == null)
        {
            if (newRelationship?.Metadata.IsInModel == true)
            {
                newRelationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(newRelationship.Metadata, configurationSource);
            }

            return null;
        }

        return relationship;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType principalEntityType,
        ConfigurationSource configurationSource,
        bool? required = null,
        string? propertyBaseName = null)
        => HasRelationshipInternal(principalEntityType, principalKey: null, configurationSource, required, propertyBaseName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasRelationship(
        EntityType principalEntityType,
        Key principalKey,
        ConfigurationSource configurationSource,
        bool? required = null,
        string? propertyBaseName = null)
        => HasRelationshipInternal(principalEntityType, principalKey, configurationSource, required, propertyBaseName);

    private InternalForeignKeyBuilder? HasRelationshipInternal(
        EntityType targetEntityType,
        Key? principalKey,
        ConfigurationSource configurationSource,
        bool? required = null,
        string? propertyBaseName = null)
    {
        InternalForeignKeyBuilder? relationship;
        InternalForeignKeyBuilder? newRelationship;
        using (var batch = Metadata.Model.DelayConventions())
        {
            relationship = CreateForeignKey(
                targetEntityType.Builder,
                null,
                principalKey,
                propertyBaseName,
                required,
                configurationSource)!;

            newRelationship = relationship;
            if (principalKey != null)
            {
                newRelationship = newRelationship.HasEntityTypes(targetEntityType, Metadata, configurationSource)
                    ?.HasPrincipalKey(principalKey.Properties, configurationSource);
            }

            newRelationship = newRelationship == null ? null : batch.Run(newRelationship);
        }

        if (newRelationship == null)
        {
            if (relationship?.Metadata.IsInModel == true)
            {
                relationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(relationship.Metadata, configurationSource);
            }

            return null;
        }

        return newRelationship;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        string targetEntityTypeName,
        string navigationName,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityTypeName),
            MemberIdentity.Create(navigationName), inverse: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        string navigationName,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityType, Metadata.Model),
            MemberIdentity.Create(navigationName), inverse: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        MemberInfo navigationMember,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityType, Metadata.Model),
            MemberIdentity.Create(navigationMember), inverse: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        MemberIdentity navigation,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityType, Metadata.Model), navigation, inverse: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        in TypeIdentity typeIdentity,
        MemberIdentity navigation,
        ConfigurationSource configurationSource)
        => HasOwnership(typeIdentity, navigation, inverse: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        string navigationPropertyName,
        string? inversePropertyName,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityType, Metadata.Model),
            MemberIdentity.Create(navigationPropertyName),
            MemberIdentity.Create(inversePropertyName),
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? HasOwnership(
        Type targetEntityType,
        MemberInfo navigationMember,
        MemberInfo? inverseMember,
        ConfigurationSource configurationSource)
        => HasOwnership(
            new TypeIdentity(targetEntityType, Metadata.Model),
            MemberIdentity.Create(navigationMember),
            MemberIdentity.Create(inverseMember),
            configurationSource);

    private InternalForeignKeyBuilder? HasOwnership(
        in TypeIdentity targetEntityType,
        in MemberIdentity navigation,
        MemberIdentity? inverse,
        ConfigurationSource configurationSource)
    {
        InternalForeignKeyBuilder? relationship;
        var existingNavigation = Metadata.FindNavigation(navigation.Name!);
        if (existingNavigation is { IsOnDependent: false })
        {
            var existingTargetType = existingNavigation.TargetEntityType;
            if ((targetEntityType.Type == null
                    || existingTargetType.ClrType == targetEntityType.Type)
                && (!targetEntityType.IsNamed
                    || targetEntityType.Name == existingTargetType.Name
                    || (targetEntityType.Type == null
                        && targetEntityType.Name == existingTargetType.ClrType.DisplayName())))
            {
                relationship = existingNavigation.ForeignKey.Builder;
                if (existingNavigation.ForeignKey.IsOwnership)
                {
                    relationship = relationship.IsOwnership(true, configurationSource)
                        ?.HasNavigations(inverse, navigation, configurationSource);

                    relationship?.Metadata.UpdateConfigurationSource(configurationSource);
                    return relationship;
                }

                Check.DebugAssert(
                    !existingTargetType.IsOwned()
                    || existingNavigation.DeclaringEntityType.IsInOwnershipPath(existingTargetType)
                    || (existingTargetType.IsInOwnershipPath(existingNavigation.DeclaringEntityType)
                        && existingTargetType.FindOwnership()!.PrincipalEntityType != existingNavigation.DeclaringEntityType),
                    $"Found '{existingNavigation.DeclaringEntityType.DisplayName()}.{existingNavigation.Name}'. "
                    + "Owned types should only have ownership or ownee navigations point at it");

                relationship = relationship.IsOwnership(true, configurationSource)
                    ?.HasNavigations(inverse, navigation, configurationSource);

                relationship?.Metadata.UpdateConfigurationSource(configurationSource);
                return relationship;
            }
        }

        InternalEntityTypeBuilder? ownedEntityTypeBuilder = null;
        using (var batch = Metadata.Model.DelayConventions())
        {
            var ownership = Metadata.FindOwnership();
            var existingDerivedNavigations = Metadata.FindDerivedNavigations(navigation.Name!)
                .Where(n => n.ForeignKey.IsOwnership).ToList();
            if (existingDerivedNavigations.Count == 1
                && existingDerivedNavigations[0].ForeignKey.DeclaringEntityType is EntityType existingOwnedType
                && !existingOwnedType.HasSharedClrType)
            {
                ownedEntityTypeBuilder = existingOwnedType.Builder;
                ownedEntityTypeBuilder.HasNoRelationship(existingDerivedNavigations[0].ForeignKey, configurationSource);
            }
            else
            {
                foreach (var existingDerivedNavigation in existingDerivedNavigations)
                {
                    ModelBuilder.HasNoEntityType(existingDerivedNavigation.DeclaringEntityType, configurationSource);
                }
            }

            if (ownedEntityTypeBuilder?.Metadata.IsInModel != true)
            {
                ownedEntityTypeBuilder = GetTargetEntityTypeBuilder(
                    targetEntityType, navigation, configurationSource, targetShouldBeOwned: true);
            }

            // TODO: Use convention batch to get the updated builder, see #15898
            var principalBuilder = Metadata.IsInModel
                ? Metadata.Builder
                : ownership?.PrincipalEntityType.FindNavigation(ownership.PrincipalToDependent!.Name)?.TargetEntityType is
                {
                    IsInModel: true
                } target
                    ? target.Builder
                    : null;

            if (ownedEntityTypeBuilder == null
                || principalBuilder == null)
            {
                Check.DebugAssert(
                    configurationSource != ConfigurationSource.Explicit,
                    $"Adding {Metadata.ShortName()}.{navigation.Name} ownership failed because one of the related types doesn't exist.");
                return null;
            }

            relationship = ownedEntityTypeBuilder.HasRelationship(
                    targetEntityType: principalBuilder.Metadata,
                    navigationToTarget: inverse,
                    inverseNavigation: navigation,
                    setTargetAsPrincipal: true,
                    configurationSource,
                    required: true)
                ?.IsOwnership(true, configurationSource);

            if (relationship == null)
            {
                batch.Dispose();
            }
            else
            {
                relationship = batch.Run(relationship);
            }
        }

        if (relationship is null || !relationship.Metadata.IsInModel)
        {
            if (ownedEntityTypeBuilder.Metadata is { IsInModel: true, HasSharedClrType: true })
            {
                ModelBuilder.HasNoEntityType(ownedEntityTypeBuilder.Metadata, configurationSource);
            }

            return null;
        }

        return relationship;
    }

    private InternalForeignKeyBuilder? HasOwnership(
        EntityType targetEntityType,
        in MemberIdentity navigation,
        MemberIdentity? inverse,
        ConfigurationSource configurationSource)
    {
        InternalForeignKeyBuilder? relationship;
        var existingNavigation = Metadata.FindNavigation(navigation.Name!);
        if (existingNavigation is { IsOnDependent: false })
        {
            var existingTargetType = existingNavigation.TargetEntityType;
            if (existingTargetType == targetEntityType)
            {
                relationship = existingNavigation.ForeignKey.Builder;
                if (existingNavigation.ForeignKey.IsOwnership)
                {
                    relationship = relationship.IsOwnership(true, configurationSource)
                        ?.HasNavigations(inverse, navigation, configurationSource);

                    relationship?.Metadata.UpdateConfigurationSource(configurationSource);
                    return relationship;
                }

                Check.DebugAssert(
                    !existingTargetType.IsOwned()
                    || existingNavigation.DeclaringEntityType.IsInOwnershipPath(existingTargetType)
                    || (existingTargetType.IsInOwnershipPath(existingNavigation.DeclaringEntityType)
                        && existingTargetType.FindOwnership()!.PrincipalEntityType != existingNavigation.DeclaringEntityType),
                    $"Found '{existingNavigation.DeclaringEntityType.DisplayName()}.{existingNavigation.Name}'. "
                    + "Owned types should only have ownership or ownee navigations point at it");

                relationship = relationship.IsOwnership(true, configurationSource)
                    ?.HasNavigations(inverse, navigation, configurationSource);

                relationship?.Metadata.UpdateConfigurationSource(configurationSource);
                return relationship;
            }
        }

        using var batch = Metadata.Model.DelayConventions();

        relationship = targetEntityType.Builder.HasRelationship(
                targetEntityType: Metadata,
                navigationToTarget: inverse,
                inverseNavigation: navigation,
                setTargetAsPrincipal: true,
                configurationSource,
                required: true)
            ?.IsOwnership(true, configurationSource);

        if (relationship == null)
        {
            batch.Dispose();
        }
        else
        {
            relationship = batch.Run(relationship);
        }

        return relationship;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoNavigation(
        Navigation navigation,
        ConfigurationSource configurationSource)
    {
        if (!CanRemoveNavigation(navigation, configurationSource))
        {
            return null;
        }

        navigation.ForeignKey.Builder.HasNavigation((string?)null, navigation.IsOnDependent, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveNavigation(Navigation navigation, ConfigurationSource configurationSource)
        => configurationSource.Overrides(navigation.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? IsOwned(
        bool owned,
        ConfigurationSource configurationSource,
        ForeignKey? futureOwnership = null)
    {
        var entityType = Metadata;
        if (entityType.IsOwned() == owned)
        {
            entityType.UpdateConfigurationSource(configurationSource);
            return this;
        }

        if (!CanSetIsOwned(owned, configurationSource))
        {
            return null;
        }

        entityType.UpdateConfigurationSource(configurationSource);
        if (owned)
        {
            entityType.SetIsOwned(true);

            HasBaseType((EntityType?)null, configurationSource);

            foreach (var derivedType in entityType.GetDirectlyDerivedTypes().ToList())
            {
                derivedType.Builder.HasBaseType((EntityType?)null, configurationSource);
            }

            if (!entityType.Builder.RemoveNonOwnershipRelationships(futureOwnership, configurationSource))
            {
                return null;
            }
        }
        else
        {
            entityType.SetIsOwned(false);

            var ownership = entityType.FindOwnership();
            if (ownership != null)
            {
                HasNoRelationship(ownership, configurationSource);
            }

            foreach (EntityType derivedType in entityType.GetDerivedTypes())
            {
                derivedType.SetIsOwned(false);
                var derivedOwnership = derivedType.FindDeclaredOwnership();
                if (derivedOwnership != null)
                {
                    derivedType.Builder.HasNoRelationship(derivedOwnership, configurationSource);
                }
            }
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsOwned(bool owned, ConfigurationSource configurationSource)
    {
        var entityType = Metadata;
        if (owned)
        {
            if (!entityType.IsOwned())
            {
                if (!configurationSource.Overrides(entityType.GetBaseTypeConfigurationSource()))
                {
                    return false;
                }

                if (!configurationSource.OverridesStrictly(entityType.GetConfigurationSource()))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                    }

                    return false;
                }
            }

            foreach (EntityType derivedType in entityType.GetDerivedTypes())
            {
                if (!derivedType.IsOwned()
                    && !configurationSource.OverridesStrictly(derivedType.GetConfigurationSource()))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClashingNonOwnedDerivedEntityType(entityType.DisplayName(), derivedType.DisplayName()));
                    }

                    return false;
                }
            }
        }
        else
        {
            if (entityType.IsOwned()
                && !configurationSource.OverridesStrictly(entityType.GetConfigurationSource()))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ClashingOwnedEntityType(entityType.DisplayName()));
                }

                return false;
            }

            foreach (EntityType derivedType in entityType.GetDerivedTypes())
            {
                if (derivedType.IsOwned()
                    && !configurationSource.OverridesStrictly(derivedType.GetConfigurationSource()))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClashingOwnedDerivedEntityType(entityType.DisplayName(), derivedType.DisplayName()));
                    }

                    return false;
                }
            }
        }

        return true;
    }

    private bool RemoveNonOwnershipRelationships(ForeignKey? futureOwnership, ConfigurationSource configurationSource)
    {
        var ownership = Metadata.FindOwnership() ?? futureOwnership;
        var incompatibleRelationships = Metadata.GetDerivedTypesInclusive().Cast<EntityType>()
            .SelectMany(t => t.GetDeclaredForeignKeys())
            .Where(
                fk => fk is { IsOwnership: false, PrincipalToDependent: not null }
                    && !Contains(ownership, fk))
            .Concat(
                Metadata.GetDerivedTypesInclusive().Cast<EntityType>()
                    .SelectMany(t => t.GetDeclaredReferencingForeignKeys())
                    .Where(
                        fk => !fk.IsOwnership
                            && !Contains(fk.DeclaringEntityType.FindOwnership(), fk)))
            .ToList();

        if (incompatibleRelationships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
        {
            return false;
        }

        foreach (var foreignKey in incompatibleRelationships)
        {
            // foreign keys can be removed by HasNoRelationship() for the other foreign key(s)
            if (foreignKey.IsInModel)
            {
                foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
            }
        }

        return true;
    }

    private static bool Contains(IReadOnlyForeignKey? inheritedFk, IReadOnlyForeignKey derivedFk)
        => inheritedFk != null
            && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
            && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? GetTargetEntityTypeBuilder(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type targetClrType,
        MemberInfo navigationInfo,
        ConfigurationSource? configurationSource,
        bool? targetShouldBeOwned = null)
        => GetTargetEntityTypeBuilder(
            new TypeIdentity(targetClrType, Metadata.Model),
            MemberIdentity.Create(navigationInfo),
            configurationSource,
            targetShouldBeOwned);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? GetTargetEntityTypeBuilder(
        TypeIdentity targetEntityType,
        MemberIdentity navigation,
        ConfigurationSource? configurationSource,
        bool? targetShouldBeOwned = null)
    {
        var existingNavigation = Metadata.FindNavigation(navigation.Name!);
        if (existingNavigation != null)
        {
            var existingTargetType = existingNavigation.TargetEntityType;
            if ((targetEntityType.Type == null
                    || existingTargetType.ClrType == targetEntityType.Type)
                && (!targetEntityType.IsNamed
                    || targetEntityType.Name == existingTargetType.Name
                    || (targetEntityType.Type == null
                        && targetEntityType.Name == existingTargetType.ClrType.DisplayName())))
            {
                Check.DebugAssert(
                    existingNavigation.ForeignKey.IsOwnership
                    || !((IReadOnlyNavigation)existingNavigation).TargetEntityType.IsOwned()
                    || existingNavigation.DeclaringEntityType.IsInOwnershipPath(existingTargetType)
                    || (existingTargetType.IsInOwnershipPath(existingNavigation.DeclaringEntityType)
                        && existingTargetType.FindOwnership()!.PrincipalEntityType != existingNavigation.DeclaringEntityType),
                    $"Found '{existingNavigation.DeclaringEntityType.DisplayName()}.{existingNavigation.Name}'. "
                    + "Owned types should only have ownership and ownee navigations point at it");

                return configurationSource == null
                    ? existingNavigation.TargetEntityType.Builder
                    : existingTargetType.HasSharedClrType
                        ? ModelBuilder.SharedTypeEntity(
                            existingTargetType.Name, existingTargetType.ClrType, configurationSource.Value, targetShouldBeOwned)
                        : ModelBuilder.Entity(existingTargetType.ClrType, configurationSource.Value, targetShouldBeOwned);
            }

            if (!targetEntityType.IsNamed
                && !existingTargetType.HasSharedClrType
                && targetEntityType.Type != null
                && targetEntityType.Type.IsAssignableFrom(existingTargetType.ClrType))
            {
                return existingNavigation.TargetEntityType.Builder;
            }
        }

        if (navigation.MemberInfo == null
            && Metadata.ClrType != Model.DefaultPropertyBagType)
        {
            if (Metadata.GetRuntimeProperties().TryGetValue(navigation.Name!, out var propertyInfo))
            {
                navigation = new MemberIdentity(propertyInfo);
            }
            else if (Metadata.GetRuntimeFields().TryGetValue(navigation.Name!, out var fieldInfo))
            {
                navigation = new MemberIdentity(fieldInfo);
            }
        }

        var targetType = targetEntityType.Type;
        if (targetType == null)
        {
            var memberType = navigation.MemberInfo?.GetMemberType();
            if (memberType != null)
            {
                targetType = memberType.TryGetSequenceType() ?? memberType;

                if (targetEntityType.Name == Metadata.Model.GetDisplayName(targetType))
                {
                    targetEntityType = new TypeIdentity(targetType, Metadata.Model);
                }
            }
        }

        targetType ??= Model.DefaultPropertyBagType;

        switch (ModelBuilder.Metadata.Configuration?.GetConfigurationType(targetType))
        {
            case null:
                break;
            case TypeConfigurationType.EntityType:
            case TypeConfigurationType.SharedTypeEntityType:
                targetShouldBeOwned ??= false;
                break;
            case TypeConfigurationType.OwnedEntityType:
                targetShouldBeOwned ??= true;
                break;
            default:
                if (configurationSource != ConfigurationSource.Explicit)
                {
                    return null;
                }
                break;
        }

        if (targetShouldBeOwned == null
            && Metadata.Model.FindIsOwnedConfigurationSource(targetType) != null)
        {
            targetShouldBeOwned = true;
        }

        if (targetShouldBeOwned == true
            || Metadata.IsOwned())
        {
            if (targetType.Equals(Metadata.ClrType)
                && configurationSource != ConfigurationSource.Explicit)
            {
                // Avoid infinite recursion on self reference
                return null;
            }
        }

        if (Metadata.IsOwned()
            && (targetShouldBeOwned != true
                || !configurationSource.Overrides(ConfigurationSource.Explicit)))
        {
            // Non-explicit relationship shouldn't create a new type if there's already
            // a compatible one in the ownership path
            var owner = (EntityType?)Metadata.FindInOwnershipPath(targetType);
            if (owner != null)
            {
                if (!configurationSource.Overrides(ConfigurationSource.Explicit)
                    && (owner.ClrType != targetType
                        || (owner.HasSharedClrType
                            && !owner.IsOwned())))
                {
                    return null;
                }
            }
            else
            {
                var ownership = Metadata.FindOwnership();
                if (ownership != null
                    && targetType.IsAssignableFrom(ownership.PrincipalEntityType.ClrType))
                {
                    owner = ownership.PrincipalEntityType;
                }
            }

            if (owner != null)
            {
                return configurationSource == null
                    ? owner.Builder
                    : owner.HasSharedClrType
                        ? ModelBuilder.SharedTypeEntity(
                            owner.Name, owner.ClrType, configurationSource.Value, targetShouldBeOwned)
                        : ModelBuilder.Entity(owner.ClrType, configurationSource.Value, targetShouldBeOwned);
            }
        }

        var targetTypeName = targetEntityType.IsNamed && (targetEntityType.Type != null || targetShouldBeOwned == false)
            ? targetEntityType.Name
            : Metadata.Model.IsShared(targetType)
                ? Metadata.GetOwnedName(
                    targetEntityType.IsNamed ? targetEntityType.Name : targetType.ShortDisplayName(), navigation.Name!)
                : Metadata.Model.GetDisplayName(targetType);

        var targetEntityTypeBuilder = ModelBuilder.Metadata.FindEntityType(targetTypeName)?.Builder;
        if (targetEntityTypeBuilder != null
            && targetEntityTypeBuilder.Metadata.IsOwned()
            && targetShouldBeOwned != false)
        {
            var existingOwnership = targetEntityTypeBuilder.Metadata.FindDeclaredOwnership();
            if (existingOwnership != null)
            {
                if (!configurationSource.Overrides(ConfigurationSource.Explicit)
                    && targetShouldBeOwned != true)
                {
                    return null;
                }

                if (targetEntityType is { IsNamed: true, Type: not null })
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClashingNamedOwnedType(
                                targetTypeName, Metadata.DisplayName(), navigation.Name));
                    }

                    return null;
                }

                if (existingOwnership.Builder.MakeDeclaringTypeShared(configurationSource) == null)
                {
                    return null;
                }

                targetEntityTypeBuilder = null;
                if (!targetEntityType.IsNamed)
                {
                    targetTypeName = Metadata.GetOwnedName(targetType.ShortDisplayName(), navigation.Name!);
                }
            }
        }

        if (configurationSource == null)
        {
            if (targetEntityTypeBuilder == null
                || (targetShouldBeOwned.HasValue
                    && targetEntityTypeBuilder.Metadata.IsOwned() != targetShouldBeOwned.Value))
            {
                return null;
            }
        }
        else if (Metadata.Model.IsShared(targetType)
                 || targetEntityType.IsNamed)
        {
            if (targetShouldBeOwned != true
                && !configurationSource.Overrides(ConfigurationSource.Explicit))
            {
                return null;
            }

            targetEntityTypeBuilder = ModelBuilder.SharedTypeEntity(
                targetTypeName, targetType, configurationSource.Value, targetShouldBeOwned);
        }
        else
        {
            if (targetEntityTypeBuilder != null
                && targetEntityTypeBuilder.Metadata.GetConfigurationSource().OverridesStrictly(configurationSource))
            {
                return targetEntityTypeBuilder;
            }

            targetEntityTypeBuilder = targetEntityType.IsNamed
                ? ModelBuilder.SharedTypeEntity(targetTypeName, targetType, configurationSource.Value, targetShouldBeOwned)
                : ModelBuilder.Entity(targetType, configurationSource.Value, targetShouldBeOwned);
        }

        return targetEntityTypeBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? CreateForeignKey(
        InternalEntityTypeBuilder principalEntityTypeBuilder,
        IReadOnlyList<Property>? dependentProperties,
        Key? principalKey,
        string? propertyBaseName,
        bool? required,
        ConfigurationSource configurationSource)
    {
        using var batch = ModelBuilder.Metadata.DelayConventions();
        var foreignKey = SetOrAddForeignKey(
            foreignKey: null, principalEntityTypeBuilder, this, dependentProperties, principalKey,
            propertyBaseName, required, configurationSource)!;

        if (foreignKey == null)
        {
            return null;
        }

        if (required.HasValue
            && foreignKey.IsRequired == required.Value)
        {
            foreignKey.SetIsRequired(required.Value, configurationSource);
        }

        return (InternalForeignKeyBuilder?)batch.Run(foreignKey)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? UpdateForeignKey(
        ForeignKey foreignKey,
        IReadOnlyList<Property>? dependentProperties,
        Key? principalKey,
        string? propertyBaseName,
        bool? isRequired,
        ConfigurationSource? configurationSource)
    {
        using var batch = ModelBuilder.Metadata.DelayConventions();
        var updatedForeignKey = SetOrAddForeignKey(
            foreignKey,
            foreignKey.PrincipalEntityType.Builder,
            foreignKey.DeclaringEntityType.Builder,
            dependentProperties,
            principalKey,
            propertyBaseName,
            isRequired,
            configurationSource)!;

        return (InternalForeignKeyBuilder?)batch.Run(updatedForeignKey)?.Builder;
    }

    private ForeignKey? SetOrAddForeignKey(
        ForeignKey? foreignKey,
        InternalEntityTypeBuilder principalEntityTypeBuilder,
        InternalEntityTypeBuilder dependentEntityTypeBuilder,
        IReadOnlyList<Property>? dependentProperties,
        Key? principalKey,
        string? propertyBaseName,
        bool? isRequired,
        ConfigurationSource? configurationSource)
    {
        var principalType = principalEntityTypeBuilder.Metadata;
        var principalBaseEntityTypeBuilder = principalType.GetRootType().Builder;
        if (principalKey == null)
        {
            if (principalType.IsKeyless
                && !configurationSource.Overrides(principalType.GetIsKeylessConfigurationSource()))
            {
                return null;
            }

            principalKey = principalType.FindPrimaryKey();
            if (principalKey != null
                && dependentProperties != null
                && (!ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalType,
                        Metadata,
                        shouldThrow: false)
                    || (foreignKey == null
                        && Metadata.FindForeignKeysInHierarchy(dependentProperties, principalKey, principalType).Any())))
            {
                principalKey = null;
            }

            if (principalKey == null
                && foreignKey != null
                && (dependentProperties == null
                    || ForeignKey.AreCompatible(
                        foreignKey.PrincipalKey.Properties,
                        dependentProperties,
                        principalType,
                        Metadata,
                        shouldThrow: false)))
            {
                principalKey = foreignKey.PrincipalKey;
            }
        }

        if (dependentProperties != null)
        {
            dependentProperties = dependentEntityTypeBuilder.GetActualProperties(dependentProperties, ConfigurationSource.Convention)!;

            if (principalKey == null)
            {
                var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                    dependentProperties.Count, null, dependentProperties.Select(p => p.ClrType),
                    Enumerable.Repeat("", dependentProperties.Count), isRequired: true, baseName: "TempId").Item2;

                if (principalKeyProperties == null)
                {
                    return null;
                }

                principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention)!
                    .Metadata;
            }
            else
            {
                Check.DebugAssert(
                    foreignKey != null
                    || Metadata.FindForeignKey(dependentProperties, principalKey, principalType) == null,
                    "FK not found");
            }
        }
        else
        {
            if (principalKey == null)
            {
                var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                    1, null, new[] { typeof(int) }, new[] { "TempId" }, isRequired: true, baseName: "").Item2;

                if (principalKeyProperties == null)
                {
                    return null;
                }

                principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(
                    principalKeyProperties, ConfigurationSource.Convention)?.Metadata;

                if (principalKey == null)
                {
                    return null;
                }
            }

            if (foreignKey != null)
            {
                var oldProperties = foreignKey.Properties;
                var oldKey = foreignKey.PrincipalKey;
                var temporaryProperties = CreateUniqueProperties(principalKey.Properties, isRequired ?? false, "TempFk");
                if (temporaryProperties == null)
                {
                    return null;
                }

                foreignKey.SetProperties(temporaryProperties, principalKey, configurationSource);

                foreignKey.DeclaringEntityType.Builder.RemoveUnusedImplicitProperties(oldProperties);
                if (oldKey != principalKey)
                {
                    oldKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(oldKey);
                }

                propertyBaseName ??= ForeignKeyPropertyDiscoveryConvention.GetPropertyBaseName(foreignKey);
            }

            var baseName = string.IsNullOrEmpty(propertyBaseName)
                ? principalType.ShortName()
                : propertyBaseName;
            dependentProperties = CreateUniqueProperties(principalKey.Properties, isRequired ?? false, baseName);
            if (dependentProperties == null)
            {
                return null;
            }
        }

        if (foreignKey == null)
        {
            return Metadata.AddForeignKey(
                dependentProperties, principalKey, principalType, componentConfigurationSource: null, configurationSource!.Value);
        }

        var oldFKProperties = foreignKey.Properties;
        var oldPrincipalKey = foreignKey.PrincipalKey;
        foreignKey.SetProperties(dependentProperties, principalKey, configurationSource);

        if (oldFKProperties != dependentProperties)
        {
            foreignKey.DeclaringEntityType.Builder.RemoveUnusedImplicitProperties(oldFKProperties);
        }

        if (oldPrincipalKey != principalKey)
        {
            oldPrincipalKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(oldPrincipalKey);
        }

        return foreignKey;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
        MemberIdentity navigation,
        EntityType targetEntityType,
        Type? navigationType,
        MemberIdentity inverseNavigation,
        Type? inverseNavigationType,
        ConfigurationSource configurationSource,
        bool? collections = null,
        bool? onDependent = null)
    {
        var skipNavigationBuilder = HasSkipNavigation(
            navigation, targetEntityType, navigationType, configurationSource, collections, onDependent);
        if (skipNavigationBuilder == null)
        {
            return null;
        }

        var inverseSkipNavigationBuilder = targetEntityType.Builder.HasSkipNavigation(
            inverseNavigation, Metadata, inverseNavigationType, configurationSource, collections, onDependent);
        if (inverseSkipNavigationBuilder == null)
        {
            HasNoSkipNavigation(skipNavigationBuilder.Metadata, configurationSource);
            return null;
        }

        return skipNavigationBuilder.HasInverse(inverseSkipNavigationBuilder.Metadata, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
        MemberInfo navigation,
        EntityType targetEntityType,
        ConfigurationSource? configurationSource,
        bool? collection = null,
        bool? onDependent = null)
        => HasSkipNavigation(
            MemberIdentity.Create(navigation),
            targetEntityType,
            navigation.GetMemberType(),
            configurationSource,
            collection,
            onDependent);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
        MemberIdentity navigation,
        EntityType targetEntityType,
        ConfigurationSource? configurationSource,
        bool? collection = null,
        bool? onDependent = null)
        => HasSkipNavigation(
            navigation,
            targetEntityType,
            navigation.MemberInfo?.GetMemberType(),
            configurationSource,
            collection,
            onDependent);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
        MemberIdentity navigation,
        EntityType targetEntityType,
        Type? navigationType,
        ConfigurationSource? configurationSource,
        bool? collection = null,
        bool? onDependent = null)
    {
        List<SkipNavigation>? navigationsToDetach = null;
        List<(InternalSkipNavigationBuilder Navigation, InternalSkipNavigationBuilder Inverse)>? detachedNavigations = null;
        InternalSkipNavigationBuilder builder;

        var navigationName = navigation.Name;
        if (navigationName != null)
        {
            var memberInfo = navigation.MemberInfo;
            navigationType ??= memberInfo?.GetMemberType();
            var existingNavigation = Metadata.FindSkipNavigation(navigationName);
            if (existingNavigation != null)
            {
                Check.DebugAssert(
                    memberInfo == null
                    || existingNavigation.IsIndexerProperty()
                    || memberInfo.IsSameAs(existingNavigation.GetIdentifyingMemberInfo()),
                    "Expected memberInfo to be the same on the existing navigation");

                Check.DebugAssert(
                    collection == null || collection == existingNavigation.IsCollection,
                    "Expected existing navigation to have the same cardinality");

                Check.DebugAssert(
                    onDependent == null || onDependent == existingNavigation.IsOnDependent,
                    "Expected existing navigation to be on the same side");

                if (existingNavigation.DeclaringEntityType != Metadata)
                {
                    if (!IsIgnored(navigationName, configurationSource))
                    {
                        Metadata.RemoveIgnored(navigationName);
                    }
                }

                if (configurationSource.HasValue)
                {
                    existingNavigation.UpdateConfigurationSource(configurationSource.Value);
                }

                return existingNavigation.Builder;
            }

            if (configurationSource != ConfigurationSource.Explicit
                && (!configurationSource.HasValue
                    || !CanAddSkipNavigation(navigationName, memberInfo?.GetMemberType(), configurationSource.Value)))
            {
                return null;
            }

            foreach (EntityType derivedType in Metadata.GetDerivedTypes())
            {
                var conflictingNavigation = derivedType.FindDeclaredSkipNavigation(navigationName);
                if (conflictingNavigation != null)
                {
                    navigationsToDetach ??= [];

                    navigationsToDetach.Add(conflictingNavigation);
                }
            }

            if (collection == null
                && navigationType != null)
            {
                var navigationTargetClrType = navigationType.TryGetSequenceType();
                collection = navigationTargetClrType != null
                    && navigationType != targetEntityType.ClrType
                    && navigationTargetClrType.IsAssignableFrom(targetEntityType.ClrType);
            }

            using (ModelBuilder.Metadata.DelayConventions())
            {
                Metadata.RemoveIgnored(navigationName);

                if (navigationsToDetach != null)
                {
                    detachedNavigations = new List<(InternalSkipNavigationBuilder, InternalSkipNavigationBuilder)>();
                    foreach (var navigationToDetach in navigationsToDetach)
                    {
                        var inverse = navigationToDetach.Inverse;
                        detachedNavigations.Add((DetachSkipNavigation(navigationToDetach)!, DetachSkipNavigation(inverse)!));
                    }
                }

                RemoveMembersInHierarchy(navigationName, configurationSource.Value);

                builder = Metadata.AddSkipNavigation(
                    navigationName, navigationType, memberInfo, targetEntityType,
                    collection ?? true, onDependent ?? false, configurationSource.Value)!.Builder;

                if (detachedNavigations != null)
                {
                    foreach (var (detachedNavigation, inverse) in detachedNavigations)
                    {
                        detachedNavigation.Attach(this, inverseBuilder: inverse);
                    }
                }
            }
        }
        else
        {
            var generatedNavigationName = targetEntityType.ShortName();
            navigationName = generatedNavigationName;
            var uniquifier = 0;
            while (Metadata.FindMembersInHierarchy(navigationName).Any())
            {
                navigationName = generatedNavigationName + (++uniquifier);
            }

            builder = Metadata.AddSkipNavigation(
                navigationName, navigationType, null, targetEntityType,
                collection ?? true, onDependent ?? false, ConfigurationSource.Explicit)!.Builder;
        }

        return builder.Metadata.IsInModel
            ? builder
            : Metadata.FindSkipNavigation(navigationName!)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasNoSkipNavigation(
        SkipNavigation skipNavigation,
        ConfigurationSource configurationSource)
    {
        if (!CanRemoveSkipNavigation(skipNavigation, configurationSource))
        {
            return null;
        }

        if (skipNavigation.Inverse != null)
        {
            var removed = skipNavigation.Inverse.Builder.HasInverse(null, configurationSource);
            Check.DebugAssert(removed != null, "Expected inverse to be removed");
        }

        if (skipNavigation.ForeignKey != null)
        {
            skipNavigation.Builder.HasForeignKey(null, configurationSource);
        }

        Metadata.RemoveSkipNavigation(skipNavigation);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveSkipNavigation(SkipNavigation skipNavigation, ConfigurationSource configurationSource)
        => configurationSource.Overrides(skipNavigation.GetConfigurationSource());

    private static InternalSkipNavigationBuilder? DetachSkipNavigation(SkipNavigation? skipNavigationToDetach)
    {
        if (skipNavigationToDetach is null || !skipNavigationToDetach.IsInModel)
        {
            return null;
        }

        var builder = skipNavigationToDetach.Builder;
        skipNavigationToDetach.DeclaringEntityType.Builder.HasNoSkipNavigation(skipNavigationToDetach, ConfigurationSource.Explicit);
        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldReuniquifyTemporaryProperties(ForeignKey foreignKey)
        => TryCreateUniqueProperties(
                foreignKey.PrincipalKey.Properties.Count,
                foreignKey.Properties,
                foreignKey.PrincipalKey.Properties.Select(p => p.ClrType),
                foreignKey.PrincipalKey.Properties.Select(p => p.Name),
                foreignKey.IsRequired
                && foreignKey.GetIsRequiredConfigurationSource().Overrides(ConfigurationSource.Convention),
                foreignKey.DependentToPrincipal?.Name
                ?? foreignKey.ReferencingSkipNavigations?.FirstOrDefault()?.Inverse?.Name
                ?? foreignKey.PrincipalEntityType.ShortName())
            .Item1;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityTypeBuilder? HasData(IEnumerable<object> data, ConfigurationSource configurationSource)
    {
        Metadata.AddData(data);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionEntityTypeBuilder? HasConstructorBinding(
        InstantiationBinding? constructorBinding,
        ConfigurationSource configurationSource)
    {
        if (CanSetConstructorBinding(constructorBinding, configurationSource))
        {
            Metadata.SetConstructorBinding(constructorBinding, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetConstructorBinding(InstantiationBinding? constructorBinding, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetConstructorBindingConfigurationSource())
            || Metadata.ConstructorBinding == constructorBinding;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionEntityTypeBuilder? HasServiceOnlyConstructorBinding(
        InstantiationBinding? constructorBinding,
        ConfigurationSource configurationSource)
    {
        if (CanSetServiceOnlyConstructorBinding(constructorBinding, configurationSource))
        {
            Metadata.SetServiceOnlyConstructorBinding(constructorBinding, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetServiceOnlyConstructorBinding(
        InstantiationBinding? constructorBinding,
        ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetServiceOnlyConstructorBindingConfigurationSource())
            || Metadata.ServiceOnlyConstructorBinding == constructorBinding;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DiscriminatorBuilder? HasDiscriminator(ConfigurationSource configurationSource)
        => DiscriminatorBuilder(
            GetOrCreateDiscriminatorProperty(type: null, name: null, ConfigurationSource.Convention),
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DiscriminatorBuilder? HasDiscriminator(
        string? name,
        Type? type,
        ConfigurationSource configurationSource)
    {
        Check.DebugAssert(name != null || type != null, $"Either {nameof(name)} or {nameof(type)} should be non-null");

        return CanSetDiscriminator(name, type, configurationSource)
            ? DiscriminatorBuilder(
                GetOrCreateDiscriminatorProperty(type, name, configurationSource),
                configurationSource)
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DiscriminatorBuilder? HasDiscriminator(MemberInfo memberInfo, ConfigurationSource configurationSource)
        => CanSetDiscriminator(
            Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName(), memberInfo.GetMemberType(), configurationSource)
            ? DiscriminatorBuilder(
                Metadata.GetRootType().Builder.Property(
                    memberInfo, configurationSource),
                configurationSource)
            : null;

    private const string DefaultDiscriminatorName = "Discriminator";

    private static readonly Type DefaultDiscriminatorType = typeof(string);

    private InternalPropertyBuilder? GetOrCreateDiscriminatorProperty(Type? type, string? name, ConfigurationSource configurationSource)
    {
        var discriminatorProperty = ((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty();
        if ((name != null && discriminatorProperty?.Name != name)
            || (type != null && discriminatorProperty?.ClrType != type))
        {
            discriminatorProperty = null;
        }

        return Metadata.GetRootType().Builder.Property(
            type ?? discriminatorProperty?.ClrType ?? DefaultDiscriminatorType,
            name ?? discriminatorProperty?.Name ?? DefaultDiscriminatorName,
            typeConfigurationSource: type != null ? configurationSource : null,
            configurationSource)?.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.Convention);
    }

    private DiscriminatorBuilder? DiscriminatorBuilder(
        InternalPropertyBuilder? discriminatorPropertyBuilder,
        ConfigurationSource configurationSource)
    {
        if (discriminatorPropertyBuilder == null)
        {
            return null;
        }

        var rootTypeBuilder = Metadata.GetRootType().Builder;
        var discriminatorProperty = discriminatorPropertyBuilder.Metadata;
        // Make sure the property is on the root type
        discriminatorPropertyBuilder = rootTypeBuilder.Property(
            discriminatorProperty.ClrType, discriminatorProperty.Name, null, ConfigurationSource.Convention)!;

        RemoveUnusedDiscriminatorProperty(discriminatorProperty, configurationSource);

        rootTypeBuilder.Metadata.SetDiscriminatorProperty(discriminatorProperty, configurationSource);

        RemoveIncompatibleDiscriminatorValues(Metadata, discriminatorProperty, configurationSource);

        discriminatorPropertyBuilder.IsRequired(true, ConfigurationSource.Convention);
        discriminatorPropertyBuilder.HasValueGeneratorFactory(
            typeof(DiscriminatorValueGeneratorFactory), ConfigurationSource.Convention);

        return new DiscriminatorBuilder(Metadata);
    }

    private void RemoveIncompatibleDiscriminatorValues(
        EntityType entityType,
        Property? newDiscriminatorProperty,
        ConfigurationSource configurationSource)
    {
        if ((newDiscriminatorProperty != null || entityType.BaseType != null)
            && (newDiscriminatorProperty == null
                || newDiscriminatorProperty.ClrType.IsInstanceOfType(((IReadOnlyEntityType)entityType).GetDiscriminatorValue())))
        {
            return;
        }

        if (configurationSource.Overrides(((IConventionEntityType)entityType).GetDiscriminatorValueConfigurationSource()))
        {
            ((IMutableEntityType)entityType).RemoveDiscriminatorValue();
        }

        if (entityType.BaseType == null)
        {
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                if (configurationSource.Overrides(((IConventionEntityType)derivedType).GetDiscriminatorValueConfigurationSource()))
                {
                    ((IMutableEntityType)derivedType).RemoveDiscriminatorValue();
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
    public virtual InternalEntityTypeBuilder? HasNoDiscriminator(ConfigurationSource configurationSource)
    {
        if (Metadata[CoreAnnotationNames.DiscriminatorProperty] == null)
        {
            return this;
        }

        if (!configurationSource.Overrides(Metadata.GetDiscriminatorPropertyConfigurationSource()))
        {
            return null;
        }

        if (((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty()?.DeclaringType == Metadata)
        {
            RemoveUnusedDiscriminatorProperty(null, configurationSource);
        }

        Metadata.SetDiscriminatorProperty(null, configurationSource);

        RemoveIncompatibleDiscriminatorValues(Metadata, null, configurationSource);

        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableEntityType)Metadata).SetDiscriminatorMappingComplete(null);
        }
        else if (CanSetAnnotation(CoreAnnotationNames.DiscriminatorMappingComplete, null, configurationSource))
        {
            ((IConventionEntityType)Metadata).SetDiscriminatorMappingComplete(
                null,
                configurationSource == ConfigurationSource.DataAnnotation);
        }

        return this;
    }

    private void RemoveUnusedDiscriminatorProperty(Property? newDiscriminatorProperty, ConfigurationSource configurationSource)
    {
        var oldDiscriminatorProperty = ((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty() as Property;
        if (oldDiscriminatorProperty?.IsInModel == true
            && oldDiscriminatorProperty != newDiscriminatorProperty)
        {
            oldDiscriminatorProperty.DeclaringType.Builder.RemoveUnusedImplicitProperties(
                new[] { oldDiscriminatorProperty });

            if (oldDiscriminatorProperty.IsInModel)
            {
                oldDiscriminatorProperty.Builder.IsRequired(null, configurationSource);
                oldDiscriminatorProperty.Builder.HasValueGenerator((Type?)null, configurationSource);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetDiscriminator(string? name, Type? type, ConfigurationSource configurationSource)
        => name == null && type == null
            ? CanRemoveDiscriminator(configurationSource)
            : CanSetDiscriminator(((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty(), name, type, configurationSource);

    private bool CanSetDiscriminator(
        IReadOnlyProperty? discriminatorProperty,
        string? name,
        Type? discriminatorType,
        ConfigurationSource configurationSource)
        => ((name == null && discriminatorType == null)
                || ((name == null || discriminatorProperty?.Name == name)
                    && (discriminatorType == null || discriminatorProperty?.ClrType == discriminatorType))
                || configurationSource.Overrides(Metadata.GetRootType().GetDiscriminatorPropertyConfigurationSource()))
            && (discriminatorProperty != null
                || Metadata.GetRootType().Builder.CanAddDiscriminatorProperty(
                    discriminatorType ?? DefaultDiscriminatorType,
                    name ?? DefaultDiscriminatorName,
                    typeConfigurationSource: discriminatorType != null
                        ? configurationSource
                        : null));

    private bool CanRemoveDiscriminator(ConfigurationSource configurationSource)
        => CanSetAnnotation(CoreAnnotationNames.DiscriminatorProperty, null, configurationSource);

    private bool CanAddDiscriminatorProperty(
        Type propertyType,
        string name,
        ConfigurationSource? typeConfigurationSource)
    {
        var conflictingProperty = Metadata.FindPropertiesInHierarchy(name).FirstOrDefault();
        if (conflictingProperty != null
            && (conflictingProperty.IsShadowProperty() || conflictingProperty.IsIndexerProperty())
            && conflictingProperty.ClrType != propertyType
            && typeConfigurationSource != null
            && !typeConfigurationSource.Overrides(conflictingProperty.GetTypeConfigurationSource()))
        {
            return false;
        }

        var memberInfo = Metadata.IsPropertyBag
            ? null
            : Metadata.ClrType.GetMembersInHierarchy(name).FirstOrDefault();

        return memberInfo == null
            || propertyType == memberInfo.GetMemberType()
            || typeConfigurationSource == null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType IConventionEntityTypeBuilder.Metadata
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
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasBaseType(
        IConventionEntityType? baseEntityType,
        bool fromDataAnnotation)
        => HasBaseType(
            (EntityType?)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetBaseType(IConventionEntityType? baseEntityType, bool fromDataAnnotation)
        => CanSetBaseType(
            (EntityType?)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder IConventionEntityTypeBuilder.RemoveUnusedImplicitProperties(
        IReadOnlyList<IConventionProperty> properties)
        => (IConventionEntityTypeBuilder)RemoveUnusedImplicitProperties(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoProperty(IConventionProperty property, bool fromDataAnnotation)
        => RemoveProperty(
                (Property)property,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            == null
                ? null
                : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoComplexProperty(
        IConventionComplexProperty complexProperty,
        bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)HasNoComplexProperty(
            (ComplexProperty)complexProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServicePropertyBuilder? IConventionEntityTypeBuilder.ServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
        => ServiceProperty(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServicePropertyBuilder? IConventionEntityTypeBuilder.ServiceProperty(
        Type serviceType,
        MemberInfo memberInfo,
        bool fromDataAnnotation)
        => ServiceProperty(
            serviceType, memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanHaveServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
        => CanHaveServiceProperty(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoServiceProperty(
        IConventionServiceProperty serviceProperty,
        bool fromDataAnnotation)
        => HasNoServiceProperty(
            (ServiceProperty)serviceProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveServiceProperty(IConventionServiceProperty serviceProperty, bool fromDataAnnotation)
        => CanRemoveServiceProperty(
            (ServiceProperty)serviceProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.Ignore(string name, bool fromDataAnnotation)
        => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKeyBuilder? IConventionEntityTypeBuilder.PrimaryKey(
        IReadOnlyList<string>? propertyNames,
        bool fromDataAnnotation)
        => PrimaryKey(
            propertyNames,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKeyBuilder? IConventionEntityTypeBuilder.PrimaryKey(
        IReadOnlyList<IConventionProperty>? properties,
        bool fromDataAnnotation)
        => PrimaryKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetPrimaryKey(IReadOnlyList<string> propertyNames, bool fromDataAnnotation)
        => CanSetPrimaryKey(
            propertyNames,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
        => CanSetPrimaryKey(
            properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKeyBuilder? IConventionEntityTypeBuilder.HasKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
        => HasKey(
            properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(bool fromDataAnnotation)
        => HasNoKey(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(
        IReadOnlyList<IConventionProperty> properties,
        bool fromDataAnnotation)
    {
        Check.NotEmpty(properties, nameof(properties));

        var key = Metadata.FindDeclaredKey(properties);
        return key != null
            ? HasNoKey(key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            : this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveKey(IConventionKey key, bool fromDataAnnotation)
        => CanRemoveKey((Key)key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(IConventionKey key, bool fromDataAnnotation)
        => HasNoKey((Key)key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveKey(bool fromDataAnnotation)
        => CanRemoveKey(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
        IReadOnlyList<string> propertyNames,
        bool fromDataAnnotation)
        => HasIndex(
            propertyNames,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
        IReadOnlyList<string> propertyNames,
        string name,
        bool fromDataAnnotation)
        => HasIndex(
            propertyNames,
            name,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanHaveIndex(IReadOnlyList<string> propertyNames, bool fromDataAnnotation)
        => CanHaveIndex(
            propertyNames,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
        IReadOnlyList<IConventionProperty> properties,
        bool fromDataAnnotation)
        => HasIndex(
            properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
        IReadOnlyList<IConventionProperty> properties,
        string name,
        bool fromDataAnnotation)
        => HasIndex(
            properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
            name,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoIndex(
        IReadOnlyList<IConventionProperty> properties,
        bool fromDataAnnotation)
    {
        Check.NotEmpty(properties, nameof(properties));

        var index = Metadata.FindDeclaredIndex(properties);
        return index != null
            ? HasNoIndex(index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            : this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoIndex(IConventionIndex index, bool fromDataAnnotation)
        => HasNoIndex((Index)index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveIndex(IConventionIndex index, bool fromDataAnnotation)
        => CanRemoveIndex((Index)index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType targetEntityType,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)targetEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType principalEntityType,
        IReadOnlyList<IConventionProperty> dependentProperties,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)principalEntityType,
            dependentProperties as IReadOnlyList<Property> ?? dependentProperties.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType principalEntityType,
        IConventionKey principalKey,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)principalEntityType,
            (Key)principalKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType principalEntityType,
        IReadOnlyList<IConventionProperty> dependentProperties,
        IConventionKey principalKey,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)principalEntityType,
            dependentProperties as IReadOnlyList<Property> ?? dependentProperties.Cast<Property>().ToList(),
            (Key)principalKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType targetEntityType,
        string navigationName,
        bool setTargetAsPrincipal,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)targetEntityType,
            navigationName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            setTargetAsPrincipal ? true : null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        bool setTargetAsPrincipal,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)targetEntityType,
            navigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            setTargetAsPrincipal ? true : null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType targetEntityType,
        string navigationName,
        string? inverseNavigationName,
        bool setTargetAsPrincipal,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)targetEntityType,
            navigationName, inverseNavigationName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            setTargetAsPrincipal);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseNavigation,
        bool setTargetAsPrincipal,
        bool fromDataAnnotation)
        => HasRelationship(
            (EntityType)targetEntityType,
            navigation, inverseNavigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            setTargetAsPrincipal);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        MemberInfo inverseNavigation,
        bool? collections,
        bool? onDependent,
        bool fromDataAnnotation)
        => HasSkipNavigation(
            MemberIdentity.Create(navigation),
            (EntityType)targetEntityType,
            navigation.GetMemberType(),
            MemberIdentity.Create(inverseNavigation),
            inverseNavigation.GetMemberType(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            collections,
            onDependent);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        Type targetEntityType,
        string navigationName,
        bool fromDataAnnotation)
        => HasOwnership(
            targetEntityType, navigationName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        IConventionEntityType targetEntityType,
        string navigationName,
        bool fromDataAnnotation)
        => HasOwnership(
            (EntityType)targetEntityType,
            MemberIdentity.Create(navigationName),
            inverse: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        Type targetEntityType,
        MemberInfo navigation,
        bool fromDataAnnotation)
        => HasOwnership(
            targetEntityType, navigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        bool fromDataAnnotation)
        => HasOwnership(
            (EntityType)targetEntityType,
            MemberIdentity.Create(navigation),
            inverse: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        Type targetEntityType,
        string navigationName,
        string? inversePropertyName,
        bool fromDataAnnotation)
        => HasOwnership(
            targetEntityType, navigationName, inversePropertyName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">The name of the navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigationName">
    ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        IConventionEntityType targetEntityType,
        string navigationName,
        string? inverseNavigationName,
        bool fromDataAnnotation)
        => HasOwnership(
            (EntityType)targetEntityType,
            MemberIdentity.Create(navigationName),
            MemberIdentity.Create(inverseNavigationName),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        Type targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseProperty,
        bool fromDataAnnotation)
        => HasOwnership(
            targetEntityType, navigation, inverseProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <param name="targetEntityType">The entity type that this relationship targets.</param>
    /// <param name="navigation">The navigation property on this entity type that is part of the relationship.</param>
    /// <param name="inverseNavigation">
    ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
    ///     is specified, the relationship will be configured without a navigation property on the target end.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the relationship if it exists on the entity type,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
        IConventionEntityType targetEntityType,
        MemberInfo navigation,
        MemberInfo? inverseNavigation,
        bool fromDataAnnotation)
        => HasOwnership(
            (EntityType)targetEntityType,
            MemberIdentity.Create(navigation),
            MemberIdentity.Create(inverseNavigation),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoRelationship(
        IReadOnlyList<IConventionProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType,
        bool fromDataAnnotation)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.NotNull(principalKey, nameof(principalKey));
        Check.NotNull(principalEntityType, nameof(principalEntityType));

        var foreignKey = Metadata.FindDeclaredForeignKey(properties, principalKey, principalEntityType);
        return foreignKey != null
            ? HasNoRelationship(foreignKey, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            : this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoRelationship(
        IConventionForeignKey foreignKey,
        bool fromDataAnnotation)
        => HasNoRelationship(
            (ForeignKey)foreignKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveRelationship(IConventionForeignKey foreignKey, bool fromDataAnnotation)
        => CanRemoveForeignKey(
            (ForeignKey)foreignKey,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanHaveNavigation(
        string navigationName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? type,
        bool fromDataAnnotation)
        => CanHaveNavigation(
            navigationName,
            type,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoNavigation(IConventionNavigation navigation, bool fromDataAnnotation)
        => HasNoNavigation(
            (Navigation)navigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveNavigation(IConventionNavigation navigation, bool fromDataAnnotation)
        => CanRemoveNavigation(
            (Navigation)navigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanHaveSkipNavigation(string skipNavigationName, Type? type, bool fromDataAnnotation)
        => CanHaveSkipNavigation(
            skipNavigationName,
            type,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        bool? collection,
        bool? onDependent,
        bool fromDataAnnotation)
        => HasSkipNavigation(
            MemberIdentity.Create(navigation),
            (EntityType)targetEntityType,
            navigation.GetMemberType(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            collection,
            onDependent);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
        string navigationName,
        IConventionEntityType targetEntityType,
        Type? navigationType,
        bool? collection,
        bool? onDependent,
        bool fromDataAnnotation)
        => HasSkipNavigation(
            MemberIdentity.Create(navigationName),
            (EntityType)targetEntityType,
            navigationType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            collection,
            onDependent);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoSkipNavigation(
        IConventionSkipNavigation skipNavigation,
        bool fromDataAnnotation)
        => HasNoSkipNavigation(
            (SkipNavigation)skipNavigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveSkipNavigation(IConventionSkipNavigation skipNavigation, bool fromDataAnnotation)
        => CanRemoveSkipNavigation(
            (SkipNavigation)skipNavigation,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionTriggerBuilder? IConventionEntityTypeBuilder.HasTrigger(string modelName, bool fromDataAnnotation)
        => HasTrigger(
            modelName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanHaveTrigger(string modelName, bool fromDataAnnotation)
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasQueryFilter(LambdaExpression? filter, bool fromDataAnnotation)
        => HasQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetQueryFilter(LambdaExpression? filter, bool fromDataAnnotation)
        => CanSetQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    [Obsolete]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasDefiningQuery(LambdaExpression? query, bool fromDataAnnotation)
        => HasDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    [Obsolete]
    bool IConventionEntityTypeBuilder.CanSetDefiningQuery(LambdaExpression? query, bool fromDataAnnotation)
        => CanSetDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)HasChangeTrackingStrategy(
            changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => (IConventionEntityTypeBuilder?)UsePropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(bool fromDataAnnotation)
        => HasDiscriminator(
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(Type type, bool fromDataAnnotation)
        => HasDiscriminator(
            name: null, Check.NotNull(type, nameof(type)),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(string name, bool fromDataAnnotation)
        => HasDiscriminator(
            Check.NotEmpty(name, nameof(name)), type: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(string name, Type type, bool fromDataAnnotation)
        => HasDiscriminator(
            Check.NotEmpty(name, nameof(name)), Check.NotNull(type, nameof(type)),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation)
        => HasDiscriminator(
            memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoDiscriminator(bool fromDataAnnotation)
        => HasNoDiscriminator(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetDiscriminator(string name, bool fromDataAnnotation)
        => CanSetDiscriminator(
            name, type: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetDiscriminator(Type type, bool fromDataAnnotation)
        => CanSetDiscriminator(
            name: null, type,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetDiscriminator(string name, Type type, bool fromDataAnnotation)
        => CanSetDiscriminator(
            name, type,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanSetDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation)
        => CanSetDiscriminator(
            Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName(), memberInfo.GetMemberType(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionEntityTypeBuilder.CanRemoveDiscriminator(bool fromDataAnnotation)
        => CanRemoveDiscriminator(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.GetTargetEntityTypeBuilder(
        Type targetClrType,
        MemberInfo navigationInfo,
        bool createIfMissing,
        bool? targetShouldBeOwned,
        bool fromDataAnnotation)
        => GetTargetEntityTypeBuilder(
            targetClrType,
            navigationInfo,
            createIfMissing
                ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                : null,
            targetShouldBeOwned);
}
