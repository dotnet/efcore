// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class InternalTypeBaseBuilder : AnnotatableBuilder<TypeBase, InternalModelBuilder>,
    IConventionTypeBaseBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalTypeBaseBuilder(TypeBase metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsCompatible(MemberInfo? newMemberInfo, PropertyBase existingProperty)
    {
        if (newMemberInfo == null)
        {
            return true;
        }

        var existingMemberInfo = existingProperty.GetIdentifyingMemberInfo();
        if (existingMemberInfo == null)
        {
            return newMemberInfo == existingProperty.DeclaringType.FindIndexerPropertyInfo();
        }

        if (newMemberInfo == existingMemberInfo)
        {
            return true;
        }

        if (!newMemberInfo.DeclaringType!.IsAssignableFrom(existingProperty.DeclaringType.ClrType))
        {
            return existingMemberInfo.IsOverriddenBy(newMemberInfo);
        }

        IMutableEntityType? existingMemberDeclaringEntityType = null;
        var declaringType = existingProperty.DeclaringType as IMutableEntityType;
        if (declaringType != null)
        {
            foreach (var baseType in declaringType.GetAllBaseTypes())
            {
                if (newMemberInfo.DeclaringType == baseType.ClrType)
                {
                    return existingMemberDeclaringEntityType != null
                        && existingMemberInfo.IsOverriddenBy(newMemberInfo);
                }

                if (existingMemberDeclaringEntityType == null
                    && existingMemberInfo.DeclaringType == baseType.ClrType)
                {
                    existingMemberDeclaringEntityType = baseType;
                }
            }
        }

        // newMemberInfo is declared on an unmapped base type, existingMemberInfo should be kept
        return newMemberInfo.IsOverriddenBy(existingMemberInfo);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? Property(
        Type? propertyType,
        string propertyName,
        ConfigurationSource? configurationSource,
        bool skipTypeCheck = false)
        => Property(
            propertyType, propertyName, memberInfo: null,
            typeConfigurationSource: configurationSource,
            configurationSource: configurationSource,
            skipTypeCheck);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? Property(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource? configurationSource,
        bool skipTypeCheck = false)
        => Property(
            propertyType, propertyName, memberInfo: null,
            typeConfigurationSource,
            configurationSource,
            skipTypeCheck);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? Property(string propertyName, ConfigurationSource? configurationSource)
        => Property(propertyType: null, propertyName, memberInfo: null, typeConfigurationSource: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? Property(MemberInfo memberInfo, ConfigurationSource? configurationSource)
        => Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), memberInfo, configurationSource, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? IndexerProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? propertyType,
        string propertyName,
        ConfigurationSource? configurationSource,
        bool skipTypeCheck = false)
    {
        var indexerPropertyInfo = Metadata.FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(propertyName, Metadata.DisplayName(), typeof(string).ShortDisplayName()));
        }

        return Property(propertyType, propertyName, indexerPropertyInfo, configurationSource, configurationSource, skipTypeCheck);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual InternalPropertyBuilder? Property(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        MemberInfo? memberInfo,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource? configurationSource,
        bool skipTypeCheck = false)
    {
        var entityType = Metadata;
        List<Property>? propertiesToDetach = null;
        var existingProperty = entityType.FindProperty(propertyName);
        if (existingProperty != null)
        {
            if (existingProperty.DeclaringType != Metadata)
            {
                if (!IsIgnored(propertyName, configurationSource))
                {
                    Metadata.RemoveIgnored(propertyName);
                }

                entityType = (EntityType)existingProperty.DeclaringType;
            }

            if (IsCompatible(memberInfo, existingProperty)
                && (propertyType == null || propertyType == existingProperty.ClrType))
            {
                if (configurationSource.HasValue)
                {
                    existingProperty.UpdateConfigurationSource(configurationSource.Value);
                }

                if (propertyType != null
                    && typeConfigurationSource.HasValue)
                {
                    existingProperty.UpdateTypeConfigurationSource(typeConfigurationSource.Value);
                }

                return existingProperty.Builder;
            }

            if (memberInfo == null
                || (memberInfo is PropertyInfo propertyInfo && propertyInfo.IsIndexerProperty()))
            {
                if (existingProperty.GetTypeConfigurationSource() is ConfigurationSource existingTypeConfigurationSource
                    && !typeConfigurationSource.Overrides(existingTypeConfigurationSource))
                {
                    return null;
                }

                memberInfo ??= existingProperty.PropertyInfo ?? (MemberInfo?)existingProperty.FieldInfo;
            }
            else if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
            {
                return null;
            }

            propertyType ??= existingProperty.ClrType;

            propertiesToDetach = [existingProperty];
        }
        else
        {
            if (configurationSource != ConfigurationSource.Explicit
                && (!configurationSource.HasValue || !CanAddProperty(propertyType ?? memberInfo?.GetMemberType(),
                    propertyName, configurationSource.Value, skipTypeCheck: skipTypeCheck)))
            {
                return null;
            }

            memberInfo ??= Metadata.IsPropertyBag
                ? null
                : Metadata.ClrType.GetMembersInHierarchy(propertyName).FirstOrDefault();

            if (propertyType == null)
            {
                if (memberInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
                typeConfigurationSource = ConfigurationSource.Explicit;
            }

            foreach (var derivedType in Metadata.GetDerivedTypes())
            {
                var derivedProperty = derivedType.FindDeclaredProperty(propertyName);
                if (derivedProperty != null)
                {
                    propertiesToDetach ??= [];

                    propertiesToDetach.Add(derivedProperty);
                }
            }
        }

        Check.DebugAssert(configurationSource is not null, "configurationSource is null");

        InternalPropertyBuilder builder;
        using (Metadata.Model.DelayConventions())
        {
            var detachedProperties = propertiesToDetach == null ? null : DetachProperties(propertiesToDetach);

            if (existingProperty == null)
            {
                Metadata.RemoveIgnored(propertyName);

                RemoveMembersInHierarchy(propertyName, configurationSource.Value);
            }

            builder = entityType.AddProperty(
                propertyName, propertyType, memberInfo, typeConfigurationSource, configurationSource.Value)!.Builder;

            detachedProperties?.Attach(this);
        }

        return builder.Metadata.IsInModel
            ? builder
            : Metadata.FindProperty(propertyName)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? PrimitiveCollection(
        Type? propertyType,
        string propertyName,
        ConfigurationSource? configurationSource)
        => PrimitiveCollection(
            propertyType, propertyName, typeConfigurationSource: configurationSource, configurationSource: configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? PrimitiveCollection(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource? configurationSource)
        => PrimitiveCollection(
            propertyType, propertyName, memberInfo: null,
            typeConfigurationSource,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? PrimitiveCollection(string propertyName, ConfigurationSource? configurationSource)
        => PrimitiveCollection(propertyType: null, propertyName, memberInfo: null, typeConfigurationSource: null, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? PrimitiveCollection(MemberInfo memberInfo, ConfigurationSource? configurationSource)
        => PrimitiveCollection(
            memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), memberInfo, configurationSource, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual InternalPropertyBuilder? PrimitiveCollection(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        MemberInfo? memberInfo,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource? configurationSource)
    {
        var builder = Property(propertyType, propertyName, memberInfo, typeConfigurationSource, configurationSource);

        if (builder != null)
        {
            var elementClrType = builder.Metadata.ClrType.TryGetElementType(typeof(IEnumerable<>));
            if (elementClrType == null)
            {
                throw new InvalidOperationException(CoreStrings.NotCollection(builder.Metadata.ClrType.ShortDisplayName(), propertyName));
            }

            builder.SetElementType(elementClrType, configurationSource!.Value);
        }

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? CreateUniqueProperty(
        Type propertyType,
        string propertyName,
        bool required,
        bool checkType = false)
        => CreateUniqueProperties(
            new[] { propertyType },
            new[] { propertyName },
            required,
            checkTypes: checkType)?.First().Builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? CreateUniqueProperties(
        IReadOnlyList<Type> propertyTypes,
        IReadOnlyList<string> propertyNames,
        bool isRequired,
        bool checkTypes = false)
        => TryCreateUniqueProperties(
            propertyNames.Count,
            null,
            propertyTypes,
            propertyNames,
            isRequired,
            "",
            checkTypes: checkTypes).Item2;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? CreateUniqueProperties(
        IReadOnlyList<Property> principalProperties,
        bool isRequired,
        string baseName,
        bool checkTypes = false)
        => TryCreateUniqueProperties(
            principalProperties.Count,
            null,
            principalProperties.Select(p => p.ClrType),
            principalProperties.Select(p => p.Name),
            isRequired,
            baseName,
            checkTypes: checkTypes).Item2;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (bool, IReadOnlyList<Property>?) TryCreateUniqueProperties(
        int propertyCount,
        IReadOnlyList<Property>? currentProperties,
        IEnumerable<Type> principalPropertyTypes,
        IEnumerable<string> principalPropertyNames,
        bool isRequired,
        string baseName,
        bool checkTypes = false)
    {
        var newProperties = currentProperties == null ? new Property[propertyCount] : null;
        var clrProperties = Metadata.GetRuntimeProperties();
        var clrFields = Metadata.GetRuntimeFields();
        var canReuniquify = false;
        using var principalPropertyNamesEnumerator = principalPropertyNames.GetEnumerator();
        using var principalPropertyTypesEnumerator = principalPropertyTypes.GetEnumerator();
        for (var i = 0;
             i < propertyCount
             && principalPropertyNamesEnumerator.MoveNext()
             && principalPropertyTypesEnumerator.MoveNext();
             i++)
        {
            var keyPropertyName = principalPropertyNamesEnumerator.Current;
            var keyPropertyType = principalPropertyTypesEnumerator.Current;

            var keyModifiedBaseName = keyPropertyName.StartsWith(baseName, StringComparison.OrdinalIgnoreCase)
                ? keyPropertyName
                : baseName + keyPropertyName;
            string propertyName;
            var clrType = keyPropertyType.MakeNullable(!isRequired);
            var index = -1;
            while (true)
            {
                propertyName = keyModifiedBaseName + (++index > 0 ? index.ToString(CultureInfo.InvariantCulture) : "");
                if (!Metadata.FindPropertiesInHierarchy(propertyName).Any()
                    && !clrProperties.ContainsKey(propertyName)
                    && !clrFields.ContainsKey(propertyName)
                    && !IsIgnored(propertyName, ConfigurationSource.Convention))
                {
                    if (currentProperties == null)
                    {
                        var propertyBuilder = Property(
                            clrType,
                            propertyName,
                            typeConfigurationSource: null,
                            configurationSource: ConfigurationSource.Convention,
                            skipTypeCheck: !checkTypes);

                        if (propertyBuilder == null)
                        {
                            return (false, null);
                        }

                        if (index > 0)
                        {
                            propertyBuilder.HasAnnotation(
                                CoreAnnotationNames.PreUniquificationName,
                                keyModifiedBaseName,
                                ConfigurationSource.Convention);
                        }

                        if (clrType.IsNullableType())
                        {
                            propertyBuilder.IsRequired(isRequired, ConfigurationSource.Convention);
                        }

                        newProperties![i] = propertyBuilder.Metadata;
                    }
                    else if (Metadata.Model.Builder.CanBeConfigured(
                                 clrType, TypeConfigurationType.Property, ConfigurationSource.Convention))
                    {
                        canReuniquify = true;
                    }

                    break;
                }

                var currentProperty = currentProperties?.SingleOrDefault(p => p.Name == propertyName);
                if (currentProperty != null)
                {
                    if (((IConventionProperty)currentProperty).IsImplicitlyCreated()
                        && currentProperty.ClrType != clrType
                        && isRequired)
                    {
                        canReuniquify = true;
                    }

                    break;
                }
            }
        }

        return (canReuniquify, newProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? GetOrCreateProperties(
        IReadOnlyList<string>? propertyNames,
        ConfigurationSource? configurationSource,
        IReadOnlyList<Property>? referencedProperties = null,
        bool required = false,
        bool useDefaultType = false)
    {
        if (propertyNames == null)
        {
            return null;
        }

        if (referencedProperties != null
            && referencedProperties.Count != propertyNames.Count)
        {
            referencedProperties = null;
        }

        var propertyList = new List<Property>();
        for (var i = 0; i < propertyNames.Count; i++)
        {
            var propertyName = propertyNames[i];
            var property = Metadata.FindProperty(propertyName);
            if (property == null)
            {
                var type = referencedProperties == null
                    ? useDefaultType
                        ? typeof(int)
                        : null
                    : referencedProperties[i].ClrType;

                if (!configurationSource.HasValue)
                {
                    return null;
                }

                var propertyBuilder = Property(
                    required
                        ? type
                        : type?.MakeNullable(),
                    propertyName,
                    typeConfigurationSource: null,
                    configurationSource.Value);

                if (propertyBuilder == null)
                {
                    return null;
                }

                property = propertyBuilder.Metadata;
            }
            else if (configurationSource.HasValue)
            {
                if (ConfigurationSource.Convention.Overrides(property.GetTypeConfigurationSource())
                    && (property.IsShadowProperty() || property.IsIndexerProperty())
                    && (!property.IsNullable || (required && property.GetIsNullableConfigurationSource() == null))
                    && property.ClrType.IsNullableType())
                {
                    property = property.DeclaringType.Builder.Property(
                            property.ClrType.MakeNullable(false),
                            property.Name,
                            configurationSource.Value)!
                        .Metadata;
                }
                else
                {
                    property = property.DeclaringType.Builder.Property(property.Name, configurationSource.Value)!.Metadata;
                }
            }

            propertyList.Add(property);
        }

        return propertyList;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? GetOrCreateProperties(
        IEnumerable<MemberInfo>? clrMembers,
        ConfigurationSource? configurationSource)
    {
        if (clrMembers == null)
        {
            return null;
        }

        var list = new List<Property>();
        foreach (var propertyInfo in clrMembers)
        {
            var propertyBuilder = Property(propertyInfo, configurationSource);
            if (propertyBuilder == null)
            {
                return null;
            }

            list.Add(propertyBuilder.Metadata);
        }

        return list;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? GetActualProperties(
        IReadOnlyList<Property>? properties,
        ConfigurationSource? configurationSource)
    {
        if (properties == null)
        {
            return null;
        }

        if (properties.Count == 0)
        {
            return properties;
        }

        for (var i = 0; ; i++)
        {
            var property = properties[i];
            if (!property.IsInModel || !property.DeclaringType.IsAssignableFrom(Metadata))
            {
                break;
            }

            if (i == properties.Count - 1)
            {
                return properties;
            }
        }

        var actualProperties = new Property[properties.Count];
        for (var i = 0; i < actualProperties.Length; i++)
        {
            var property = properties[i];
            var typeConfigurationSource = property.GetTypeConfigurationSource();
            var builder = Property(
                typeConfigurationSource.Overrides(ConfigurationSource.DataAnnotation)
                    || (property.IsInModel && Metadata.IsAssignableFrom(property.DeclaringType))
                    ? property.ClrType
                    : null,
                property.Name,
                property.GetIdentifyingMemberInfo(),
                typeConfigurationSource.Overrides(ConfigurationSource.DataAnnotation) ? typeConfigurationSource : null,
                configurationSource);

            if (builder == null)
            {
                return null;
            }

            actualProperties[i] = builder.Metadata;
        }

        return actualProperties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PropertiesSnapshot? DetachProperties(IReadOnlyList<Property> propertiesToDetach)
    {
        if (propertiesToDetach.Count == 0)
        {
            return null;
        }

        List<RelationshipSnapshot>? detachedRelationships = null;
        foreach (var propertyToDetach in propertiesToDetach)
        {
            foreach (var relationship in propertyToDetach.GetContainingForeignKeys().ToList())
            {
                detachedRelationships ??= [];

                detachedRelationships.Add(InternalEntityTypeBuilder.DetachRelationship(relationship));
            }
        }

        var detachedIndexes =
            InternalEntityTypeBuilder.DetachIndexes(propertiesToDetach.SelectMany(p => p.GetContainingIndexes()).Distinct());

        var keysToDetach = propertiesToDetach.SelectMany(p => p.GetContainingKeys()).Distinct().ToList();
        foreach (var key in keysToDetach)
        {
            foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
            {
                detachedRelationships ??= [];

                detachedRelationships.Add(InternalEntityTypeBuilder.DetachRelationship(referencingForeignKey));
            }
        }

        var detachedKeys = InternalEntityTypeBuilder.DetachKeys(keysToDetach);

        var detachedProperties = new List<InternalPropertyBuilder>();
        foreach (var propertyToDetach in propertiesToDetach)
        {
            var property = propertyToDetach.DeclaringType.FindDeclaredProperty(propertyToDetach.Name);
            if (property != null)
            {
                var propertyBuilder = property.Builder;
                // Reset convention configuration
                propertyBuilder.ValueGenerated(null, ConfigurationSource.Convention);
                propertyBuilder.AfterSave(null, ConfigurationSource.Convention);
                propertyBuilder.BeforeSave(null, ConfigurationSource.Convention);
                ConfigurationSource? removedConfigurationSource;
                if (property.DeclaringType.IsInModel)
                {
                    removedConfigurationSource = property.DeclaringType.Builder
                        .RemoveProperty(property, property.GetConfigurationSource());
                }
                else
                {
                    removedConfigurationSource = property.GetConfigurationSource();
                    property.DeclaringType.RemoveProperty(property.Name);
                }

                Check.DebugAssert(removedConfigurationSource.HasValue, "removedConfigurationSource.HasValue is false");
                detachedProperties.Add(propertyBuilder);
            }
        }

        return new PropertiesSnapshot(detachedProperties, detachedIndexes, detachedKeys, detachedRelationships);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void RemoveMembersInHierarchy(string propertyName, ConfigurationSource configurationSource)
    {
        foreach (var conflictingProperty in Metadata.FindPropertiesInHierarchy(propertyName))
        {
            if (conflictingProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
            {
                conflictingProperty.DeclaringType.RemoveProperty(conflictingProperty);
            }
        }

        foreach (var conflictingComplexProperty in Metadata.FindComplexPropertiesInHierarchy(propertyName))
        {
            if (conflictingComplexProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
            {
                conflictingComplexProperty.DeclaringType.RemoveComplexProperty(conflictingComplexProperty);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        MemberInfo? memberInfo,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource? configurationSource,
        bool checkClrProperty = false)
    {
        var existingProperty = Metadata.FindProperty(propertyName);
        return existingProperty != null
            ? (IsCompatible(memberInfo, existingProperty)
                && (propertyType == null || propertyType == existingProperty.ClrType))
            || ((memberInfo == null
                    || (memberInfo is PropertyInfo propertyInfo && propertyInfo.IsIndexerProperty()))
                && (existingProperty.GetTypeConfigurationSource() is not ConfigurationSource existingTypeConfigurationSource
                    || typeConfigurationSource.Overrides(existingTypeConfigurationSource)))
            || configurationSource.Overrides(existingProperty.GetConfigurationSource())
            : configurationSource.HasValue
            && CanAddProperty(propertyType ?? memberInfo?.GetMemberType(),
                propertyName, configurationSource.Value, checkClrProperty: checkClrProperty);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract bool CanAddProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        ConfigurationSource configurationSource,
        bool checkClrProperty = false,
        bool skipTypeCheck = false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveProperty(
        Property property,
        ConfigurationSource configurationSource,
        bool canOverrideSameSource = true)
    {
        Check.NotNull(property, nameof(property));
        Check.DebugAssert(property.DeclaringType == Metadata, "property.DeclaringEntityType != Metadata");

        var currentConfigurationSource = property.GetConfigurationSource();
        return configurationSource.Overrides(currentConfigurationSource)
            && (canOverrideSameSource || (configurationSource != currentConfigurationSource));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? RemoveProperty(
        Property property,
        ConfigurationSource configurationSource,
        bool canOverrideSameSource = true)
    {
        var currentConfigurationSource = property.GetConfigurationSource();
        if (!configurationSource.Overrides(currentConfigurationSource)
            || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            var detachedRelationships = property.GetContainingForeignKeys().ToList()
                .Select(InternalEntityTypeBuilder.DetachRelationship).ToList();

            foreach (var key in property.GetContainingKeys().ToList())
            {
                detachedRelationships.AddRange(
                    key.GetReferencingForeignKeys().ToList()
                        .Select(InternalEntityTypeBuilder.DetachRelationship));
                var removed = key.DeclaringEntityType.Builder.HasNoKey(key, configurationSource);
                Check.DebugAssert(removed != null, "removed is null");
            }

            foreach (var index in property.GetContainingIndexes().ToList())
            {
                var removed = index.DeclaringEntityType.Builder.HasNoIndex(index, configurationSource);
                Check.DebugAssert(removed != null, "removed is null");
            }

            if (property.IsInModel)
            {
                var removedProperty = Metadata.RemoveProperty(property.Name);
                Check.DebugAssert(removedProperty == property, "removedProperty != property");
            }

            foreach (var relationshipSnapshot in detachedRelationships)
            {
                relationshipSnapshot.Attach();
            }
        }

        return currentConfigurationSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTypeBaseBuilder RemoveUnusedImplicitProperties(IReadOnlyList<IConventionProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.IsInModel && property.IsImplicitlyCreated())
            {
                RemovePropertyIfUnused((Property)property, ConfigurationSource.Convention);
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
    protected virtual void RemovePropertyIfUnused(Property property, ConfigurationSource configurationSource)
    {
        if (!property.IsInModel
            || !property.DeclaringType.Builder.CanRemoveProperty(property, configurationSource)
            || property.GetContainingIndexes().Any()
            || property.GetContainingForeignKeys().Any()
            || property.GetContainingKeys().Any())
        {
            return;
        }

        var removedProperty = property.DeclaringType.RemoveProperty(property.Name);
        Check.DebugAssert(removedProperty == property, "removedProperty != property");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? ComplexIndexerProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? propertyType,
        string propertyName,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? complexType,
        bool? collection,
        ConfigurationSource? configurationSource)
    {
        var indexerPropertyInfo = Metadata.FindIndexerPropertyInfo();
        if (indexerPropertyInfo == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NonIndexerEntityType(propertyName, Metadata.DisplayName(), typeof(string).ShortDisplayName()));
        }

        return ComplexProperty(
            propertyType, propertyName, indexerPropertyInfo, complexTypeName: null, complexType, collection, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? ComplexProperty(
        MemberInfo memberInfo,
        string? complexTypeName,
        bool? collection,
        ConfigurationSource? configurationSource)
        => ComplexProperty(
            memberInfo.GetMemberType(), memberInfo.Name, memberInfo, complexTypeName,
            complexType: null, collection, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? ComplexProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? propertyType,
        string propertyName,
        string? complexTypeName,
        bool? collection,
        ConfigurationSource? configurationSource)
        => ComplexProperty(
            propertyType, propertyName, memberInfo: null, complexTypeName, complexType: null, collection, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? ComplexProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? propertyType,
        string propertyName,
        MemberInfo? memberInfo,
        string? complexTypeName,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type? complexType,
        bool? collection,
        ConfigurationSource? configurationSource)
    {
        var typeBase = Metadata;
        List<ComplexProperty>? propertiesToDetach = null;
        var existingComplexProperty = Metadata.FindComplexProperty(propertyName);
        if (existingComplexProperty != null)
        {
            if (existingComplexProperty.DeclaringType != Metadata)
            {
                if (!IsIgnored(propertyName, configurationSource))
                {
                    Metadata.RemoveIgnored(propertyName);
                }

                typeBase = (EntityType)existingComplexProperty.DeclaringType;
            }

            var existingComplexType = existingComplexProperty.ComplexType;
            if (IsCompatible(memberInfo, existingComplexProperty)
                && (propertyType == null
                    || existingComplexProperty.ClrType == propertyType)
                && (complexType == null
                    || existingComplexType.ClrType == complexType)
                && (collection == null
                    || collection.Value == existingComplexProperty.IsCollection))
            {
                if (configurationSource.HasValue)
                {
                    existingComplexProperty.UpdateConfigurationSource(configurationSource.Value);
                }

                return existingComplexProperty.Builder;
            }

            if (!configurationSource.Overrides(existingComplexProperty.GetConfigurationSource()))
            {
                return null;
            }

            Debug.Assert(configurationSource.HasValue);

            memberInfo ??= existingComplexProperty.PropertyInfo ?? (MemberInfo?)existingComplexProperty.FieldInfo;
            propertyType ??= existingComplexProperty.ClrType;
            collection ??= existingComplexProperty.IsCollection;
            complexType ??= existingComplexType.ClrType;

            propertiesToDetach = [existingComplexProperty];
        }
        else
        {
            if (configurationSource != ConfigurationSource.Explicit
                && (!configurationSource.HasValue
                    || !CanAddComplexProperty(
                        propertyName, propertyType ?? memberInfo?.GetMemberType(), complexType, collection, configurationSource.Value)))
            {
                return null;
            }

            memberInfo ??= Metadata.IsPropertyBag
                ? null
                : Metadata.ClrType.GetMembersInHierarchy(propertyName).FirstOrDefault();

            if (propertyType == null)
            {
                if (memberInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
            }

            if (collection == false)
            {
                complexType = propertyType;
            }

            if (collection == null
                || complexType == null)
            {
                var elementType = propertyType.TryGetSequenceType();
                collection ??= elementType != null;
                complexType ??= collection.Value ? elementType : propertyType;
            }

            foreach (var derivedType in Metadata.GetDerivedTypes())
            {
                var derivedProperty = derivedType.FindDeclaredComplexProperty(propertyName);
                if (derivedProperty != null)
                {
                    propertiesToDetach ??= [];

                    propertiesToDetach.Add(derivedProperty);
                }
            }
        }

        var model = Metadata.Model;
        ComplexProperty complexProperty;
        using (model.DelayConventions())
        {
            var detachedProperties = propertiesToDetach == null ? null : DetachProperties(propertiesToDetach);
            if (existingComplexProperty == null)
            {
                Metadata.RemoveIgnored(propertyName);

                RemoveMembersInHierarchy(propertyName, configurationSource.Value);
            }

            complexProperty = typeBase.AddComplexProperty(
                propertyName, propertyType, memberInfo, complexTypeName, complexType!, collection.Value, configurationSource.Value)!;

            if (detachedProperties != null)
            {
                foreach (var detachedProperty in detachedProperties)
                {
                    detachedProperty.Attach(this);
                }
            }
        }

        return complexProperty.IsInModel
            ? complexProperty.Builder
            : Metadata.FindComplexProperty(propertyName)?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveComplexProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        string propertyName,
        MemberInfo? memberInfo,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? complexType,
        bool? collection,
        ConfigurationSource? configurationSource)
    {
        propertyType ??= memberInfo?.GetMemberType();
        var existingComplexProperty = Metadata.FindComplexProperty(propertyName);
        var existingComplexType = existingComplexProperty?.ComplexType;
        return existingComplexProperty != null
            ? (IsCompatible(memberInfo, existingComplexProperty)
                && (propertyType == null
                    || existingComplexProperty.ClrType == propertyType)
                && (complexType == null
                    || existingComplexType!.ClrType == complexType)
                && (collection == null
                    || collection.Value == existingComplexProperty.IsCollection))
            || configurationSource.Overrides(existingComplexProperty.GetConfigurationSource())
            : configurationSource.HasValue
            && CanAddComplexProperty(propertyName, propertyType, complexType, collection, configurationSource.Value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool CanAddComplexProperty(
        string propertyName,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? propertyType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? targetType,
        bool? collection,
        ConfigurationSource configurationSource,
        bool checkClrProperty = false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTypeBaseBuilder? HasNoComplexProperty(
        ComplexProperty complexProperty,
        ConfigurationSource configurationSource)
    {
        if (!CanRemoveComplexProperty(complexProperty, configurationSource))
        {
            return null;
        }

        Metadata.RemoveComplexProperty(complexProperty);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanRemoveComplexProperty(ComplexProperty complexProperty, ConfigurationSource configurationSource)
        => configurationSource.Overrides(complexProperty.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static List<ComplexPropertySnapshot>? DetachProperties(IReadOnlyList<ComplexProperty> propertiesToDetach)
    {
        if (propertiesToDetach.Count == 0)
        {
            return null;
        }

        var detachedProperties = new List<ComplexPropertySnapshot>();
        foreach (var propertyToDetach in propertiesToDetach)
        {
            var snapshot = InternalComplexPropertyBuilder.Detach(propertyToDetach);
            if (snapshot == null)
            {
                continue;
            }

            detachedProperties.Add(snapshot);
        }

        return detachedProperties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsIgnored(string name, ConfigurationSource? configurationSource)
    {
        Check.NotEmpty(name, nameof(name));

        return configurationSource != ConfigurationSource.Explicit
            && !configurationSource.OverridesStrictly(Metadata.FindIgnoredConfigurationSource(name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract InternalTypeBaseBuilder? Ignore(string name, ConfigurationSource configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanIgnore(string name, ConfigurationSource configurationSource)
        => CanIgnore(name, configurationSource, shouldThrow: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract bool CanIgnore(string name, ConfigurationSource configurationSource, bool shouldThrow);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTypeBaseBuilder? HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        ConfigurationSource configurationSource)
    {
        if (CanSetChangeTrackingStrategy(changeTrackingStrategy, configurationSource))
        {
            Metadata.SetChangeTrackingStrategy(changeTrackingStrategy, configurationSource);

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
    public virtual bool CanSetChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetChangeTrackingStrategyConfigurationSource())
            || Metadata.GetChangeTrackingStrategy() == changeTrackingStrategy;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTypeBaseBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
    {
        if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
        {
            Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

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
    public virtual bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
        => configurationSource.Overrides(((IConventionTypeBase)Metadata).GetPropertyAccessModeConfigurationSource())
            || ((IConventionTypeBase)Metadata).GetPropertyAccessMode() == propertyAccessMode;

    IConventionTypeBase IConventionTypeBaseBuilder.Metadata
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
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionTypeBaseBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionTypeBaseBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionTypeBaseBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionPropertyBuilder? IConventionTypeBaseBuilder.Property(MemberInfo memberInfo, bool fromDataAnnotation)
        => Property(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionPropertyBuilder? IConventionTypeBaseBuilder.Property(
        Type propertyType,
        string propertyName,
        bool setTypeConfigurationSource,
        bool fromDataAnnotation)
        => Property(
            propertyType,
            propertyName, setTypeConfigurationSource
                ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                : null, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveProperty(
        Type? propertyType,
        string propertyName,
        bool fromDataAnnotation)
        => CanHaveProperty(
            propertyType,
            propertyName,
            null,
            propertyType != null
                ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                : null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveProperty(MemberInfo memberInfo, bool fromDataAnnotation)
        => CanHaveProperty(
            memberInfo.GetMemberType(),
            memberInfo.Name,
            memberInfo,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionPropertyBuilder? IConventionTypeBaseBuilder.IndexerProperty(
        Type propertyType,
        string propertyName,
        bool fromDataAnnotation)
        => Property(
            propertyType,
            propertyName,
            Metadata.FindIndexerPropertyInfo(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveIndexerProperty(
        Type propertyType,
        string propertyName,
        bool fromDataAnnotation)
        => CanHaveProperty(
            propertyType,
            propertyName,
            Metadata.FindIndexerPropertyInfo(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionPropertyBuilder? IConventionTypeBaseBuilder.CreateUniqueProperty(
        Type propertyType,
        string basePropertyName,
        bool required)
        => CreateUniqueProperty(propertyType, basePropertyName, required);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyList<IConventionProperty>? IConventionTypeBaseBuilder.GetOrCreateProperties(
        IReadOnlyList<string>? propertyNames,
        bool fromDataAnnotation)
        => GetOrCreateProperties(
            propertyNames, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyList<IConventionProperty>? IConventionTypeBaseBuilder.GetOrCreateProperties(
        IEnumerable<MemberInfo>? memberInfos,
        bool fromDataAnnotation)
        => GetOrCreateProperties(memberInfos, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder IConventionTypeBaseBuilder.RemoveUnusedImplicitProperties(
        IReadOnlyList<IConventionProperty> properties)
        => RemoveUnusedImplicitProperties(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasNoProperty(IConventionProperty property, bool fromDataAnnotation)
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
    bool IConventionTypeBaseBuilder.CanRemoveProperty(IConventionProperty property, bool fromDataAnnotation)
        => CanRemoveProperty(
            (Property)property,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionTypeBaseBuilder.ComplexProperty(
        Type propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation)
        => ComplexProperty(
            propertyType,
            propertyName,
            memberInfo: null,
            complexTypeName: null,
            complexType: complexType,
            collection: null,
            configurationSource: fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionTypeBaseBuilder.ComplexProperty(
        MemberInfo memberInfo,
        Type? complexType,
        bool fromDataAnnotation)
        => ComplexProperty(
            propertyType: memberInfo.GetMemberType(),
            propertyName: memberInfo.Name,
            memberInfo: memberInfo,
            complexTypeName: null,
            complexType: complexType,
            collection: null,
            configurationSource: fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveComplexProperty(
        Type? propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation)
        => CanHaveComplexProperty(
            propertyType,
            propertyName,
            memberInfo: null,
            complexType,
            collection: null,
            configurationSource: fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveComplexProperty(MemberInfo memberInfo, Type? complexType, bool fromDataAnnotation)
        => CanHaveComplexProperty(
            memberInfo.GetMemberType(),
            memberInfo.Name,
            memberInfo,
            complexType,
            collection: null,
            configurationSource: fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexPropertyBuilder? IConventionTypeBaseBuilder.ComplexIndexerProperty(
        Type propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation)
        => ComplexIndexerProperty(
            propertyType,
            propertyName,
            complexType,
            collection: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanHaveComplexIndexerProperty(
        Type propertyType,
        string propertyName,
        Type? complexType,
        bool fromDataAnnotation)
        => CanHaveComplexProperty(
            propertyType,
            propertyName,
            Metadata.FindIndexerPropertyInfo(),
            complexType,
            collection: null,
            configurationSource: fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasNoComplexProperty(
        IConventionComplexProperty complexProperty,
        bool fromDataAnnotation)
        => HasNoComplexProperty(
            (ComplexProperty)complexProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanRemoveComplexProperty(IConventionComplexProperty complexProperty, bool fromDataAnnotation)
        => CanRemoveComplexProperty(
            (ComplexProperty)complexProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionTypeBaseBuilder.IsIgnored(string name, bool fromDataAnnotation)
        => IsIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.Ignore(string memberName, bool fromDataAnnotation)
        => Ignore(memberName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanIgnore(string name, bool fromDataAnnotation)
        => CanIgnore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation)
        => HasChangeTrackingStrategy(
            changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IConventionTypeBaseBuilder.CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation)
        => CanSetChangeTrackingStrategy(
            changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTypeBaseBuilder? IConventionTypeBaseBuilder.UsePropertyAccessMode(
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
    bool IConventionTypeBaseBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
