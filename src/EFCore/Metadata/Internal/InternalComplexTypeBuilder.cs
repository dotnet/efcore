// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalComplexTypeBuilder : InternalTypeBaseBuilder, IConventionComplexTypeBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalComplexTypeBuilder(ComplexType metadata, InternalModelBuilder modelBuilder)
        : base(metadata, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual ComplexType Metadata
        => (ComplexType)base.Metadata;

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
            && Metadata.FindComplexPropertiesInHierarchy(propertyName)
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

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
            && Metadata.FindPropertiesInHierarchy(propertyName)
                .All(
                    m => configurationSource.Overrides(m.GetConfigurationSource())
                        && m.GetConfigurationSource() != ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override InternalComplexTypeBuilder? Ignore(string name, ConfigurationSource configurationSource)
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

            var property = Metadata.FindProperty(name);
            if (property != null)
            {
                Check.DebugAssert(property.DeclaringType == Metadata, "property.DeclaringComplexType != ComplexType");

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
                    Check.DebugAssert(complexProperty.DeclaringType == Metadata, "property.DeclaringType != ComplexType");

                    if (complexProperty.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        ModelBuilder.Metadata.ScopedModelDependencies?.Logger.MappedComplexPropertyIgnoredWarning(complexProperty);
                    }

                    var removedComplexProperty = Metadata.RemoveComplexProperty(complexProperty);

                    Check.DebugAssert(removedComplexProperty != null, "removedProperty is null");
                }
            }

            foreach (var derivedType in Metadata.GetDerivedTypes())
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
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexTypeBuilder? HasBaseType(
        ComplexType? baseComplexType,
        ConfigurationSource configurationSource)
    {
        if (Metadata.BaseType == baseComplexType)
        {
            Metadata.SetBaseType(baseComplexType, configurationSource);
            return this;
        }

        if (!CanSetBaseType(baseComplexType, configurationSource))
        {
            return null;
        }

        using (Metadata.Model.DelayConventions())
        {
            PropertiesSnapshot? detachedProperties = null;
            List<ComplexPropertySnapshot>? detachedComplexProperties = null;
            // We use at least DataAnnotation as ConfigurationSource while removing to allow us
            // to remove metadata object which were defined in derived type
            // while corresponding annotations were present on properties in base type.
            var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
            if (baseComplexType != null)
            {
                var baseMemberNames = baseComplexType.GetMembers()
                    .ToDictionary(m => m.Name, m => (ConfigurationSource?)m.GetConfigurationSource());

                var propertiesToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties()),
                        baseMemberNames,
                        p => baseComplexType.FindProperty(p.Name) != null,
                        p => p.DeclaringType.Builder.RemoveProperty(p, ConfigurationSource.Explicit));

                if (propertiesToDetach != null)
                {
                    detachedProperties = DetachProperties(propertiesToDetach);
                }

                var complexPropertiesToDetach =
                    FindConflictingMembers(
                        Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredComplexProperties()),
                        baseMemberNames,
                        p => baseComplexType.FindComplexProperty(p.Name) != null,
                        p => p.DeclaringType.RemoveComplexProperty(p));

                if (complexPropertiesToDetach != null)
                {
                    detachedComplexProperties = [];
                    foreach (var complexPropertyToDetach in complexPropertiesToDetach)
                    {
                        detachedComplexProperties.Add(InternalComplexPropertyBuilder.Detach(complexPropertyToDetach)!);
                    }
                }

                foreach (var ignoredMember in Metadata.GetIgnoredMembers().ToList())
                {
                    if (baseComplexType.FindIgnoredConfigurationSource(ignoredMember)
                        .Overrides(Metadata.FindDeclaredIgnoredConfigurationSource(ignoredMember)))
                    {
                        Metadata.RemoveIgnored(ignoredMember);
                    }
                }

                baseComplexType.UpdateConfigurationSource(configurationSource);
            }

            Metadata.SetBaseType(baseComplexType, configurationSource);

            if (detachedComplexProperties != null)
            {
                foreach (var detachedComplexProperty in detachedComplexProperties)
                {
                    detachedComplexProperty.Attach(
                        detachedComplexProperty.ComplexProperty.DeclaringType.Builder);
                }
            }

            detachedProperties?.Attach(this);
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
                            baseComplexType.FindIgnoredConfigurationSource(member.Name))
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
                                baseComplexType.DisplayName(),
                                ((IReadOnlyTypeBase)member.DeclaringType).DisplayName(),
                                member.Name,
                                baseComplexType.DisplayName(),
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
    public virtual bool CanSetBaseType(ComplexType? baseComplexType, ConfigurationSource configurationSource)
    {
        if (Metadata.BaseType == baseComplexType
            || configurationSource == ConfigurationSource.Explicit)
        {
            return true;
        }

        if (!configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource()))
        {
            return false;
        }

        if (baseComplexType == null)
        {
            return true;
        }

        var baseMembers = baseComplexType.GetMembers()
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
    public virtual InternalComplexTypeBuilder? HasConstructorBinding(
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
    public virtual InternalComplexTypeBuilder? HasServiceOnlyConstructorBinding(
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

    IConventionComplexType IConventionComplexTypeBuilder.Metadata
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
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder IConventionComplexTypeBuilder.RemoveUnusedImplicitProperties(
        IReadOnlyList<IConventionProperty> properties)
        => (IConventionComplexTypeBuilder)RemoveUnusedImplicitProperties(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasNoProperty(IConventionProperty property, bool fromDataAnnotation)
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
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasNoComplexProperty(
        IConventionComplexProperty complexProperty,
        bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)HasNoComplexProperty(
            (ComplexProperty)complexProperty,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.Ignore(string name, bool fromDataAnnotation)
        => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)HasChangeTrackingStrategy(
            changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexTypeBuilder? IConventionComplexTypeBuilder.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => (IConventionComplexTypeBuilder?)UsePropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
