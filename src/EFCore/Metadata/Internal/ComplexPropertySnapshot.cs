// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ComplexPropertySnapshot
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ComplexPropertySnapshot(
        InternalComplexPropertyBuilder complexPropertyBuilder,
        PropertiesSnapshot? properties,
        List<InternalIndexBuilder>? indexes,
        List<(InternalKeyBuilder, ConfigurationSource?)>? keys,
        List<RelationshipSnapshot>? relationships)
    {
        ComplexPropertyBuilder = complexPropertyBuilder;
        ComplexTypeBuilder = ComplexProperty.ComplexType.Builder;
        Properties = properties ?? new PropertiesSnapshot(null, null, null, null);
        if (indexes != null)
        {
            Properties.Add(indexes);
        }

        if (keys != null)
        {
            Properties.Add(keys);
        }

        if (relationships != null)
        {
            Properties.Add(relationships);
        }
    }

    private InternalComplexPropertyBuilder ComplexPropertyBuilder { [DebuggerStepThrough] get; }
    private InternalComplexTypeBuilder ComplexTypeBuilder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty ComplexProperty
        => ComplexPropertyBuilder.Metadata;

    private ComplexType ComplexType
        => ComplexTypeBuilder.Metadata;

    private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalComplexPropertyBuilder? Attach(InternalTypeBaseBuilder typeBaseBuilder)
    {
        var newProperty = typeBaseBuilder.Metadata.FindComplexProperty(ComplexProperty.Name);
        if (newProperty == ComplexProperty)
        {
            return newProperty.Builder;
        }

        InternalComplexPropertyBuilder? complexPropertyBuilder;
        var configurationSource = ComplexProperty.GetConfigurationSource();
        if (newProperty != null
            && (newProperty.GetConfigurationSource().Overrides(configurationSource)
                || (ComplexProperty.ClrType == newProperty.ClrType
                    && ComplexProperty.Name == newProperty.Name
                    && ComplexProperty.GetIdentifyingMemberInfo() == newProperty.GetIdentifyingMemberInfo())))
        {
            complexPropertyBuilder = newProperty.Builder;
            newProperty.UpdateConfigurationSource(configurationSource);
        }
        else
        {
            complexPropertyBuilder = ComplexProperty.IsIndexerProperty()
                ? typeBaseBuilder.ComplexIndexerProperty(
                    ComplexProperty.ClrType,
                    ComplexProperty.Name,
                    ComplexType.ClrType,
                    ComplexProperty.IsCollection,
                    configurationSource)
                : typeBaseBuilder.ComplexProperty(
                    ComplexProperty.ClrType,
                    ComplexProperty.Name,
                    ComplexProperty.GetIdentifyingMemberInfo(),
                    ComplexType.Name,
                    ComplexType.ClrType,
                    ComplexProperty.IsCollection,
                    configurationSource);

            if (complexPropertyBuilder is null)
            {
                return null;
            }
        }

        return MergeConfiguration(complexPropertyBuilder);
    }

    private InternalComplexPropertyBuilder MergeConfiguration(InternalComplexPropertyBuilder complexPropertyBuilder)
    {
        complexPropertyBuilder.MergeAnnotationsFrom(ComplexProperty);

        var oldIsNullableConfigurationSource = ComplexProperty.GetIsNullableConfigurationSource();
        if (oldIsNullableConfigurationSource.HasValue)
        {
            complexPropertyBuilder.IsRequired(!ComplexProperty.IsNullable, oldIsNullableConfigurationSource.Value);
        }

        var oldPropertyAccessModeConfigurationSource = ComplexProperty.GetPropertyAccessModeConfigurationSource();
        if (oldPropertyAccessModeConfigurationSource.HasValue)
        {
            complexPropertyBuilder.UsePropertyAccessMode(
                ((IReadOnlyProperty)ComplexProperty).GetPropertyAccessMode(), oldPropertyAccessModeConfigurationSource.Value);
        }

        var oldFieldInfoConfigurationSource = ComplexProperty.GetFieldInfoConfigurationSource();
        if (oldFieldInfoConfigurationSource.HasValue
            && complexPropertyBuilder.CanSetField(ComplexProperty.FieldInfo, oldFieldInfoConfigurationSource))
        {
            complexPropertyBuilder.HasField(ComplexProperty.FieldInfo, oldFieldInfoConfigurationSource.Value);
        }

        complexPropertyBuilder.MergeAnnotationsFrom(ComplexProperty);
        if (ComplexProperty.GetIsNullableConfigurationSource() != null)
        {
            complexPropertyBuilder.IsRequired(!ComplexProperty.IsNullable, ComplexProperty.GetIsNullableConfigurationSource()!.Value);
        }

        var complexTypeBuilder = complexPropertyBuilder.Metadata.ComplexType.Builder;
        complexTypeBuilder.MergeAnnotationsFrom(ComplexType);

        foreach (var ignoredMember in ComplexType.GetIgnoredMembers())
        {
            complexTypeBuilder.Ignore(ignoredMember, ComplexType.FindDeclaredIgnoredConfigurationSource(ignoredMember)!.Value);
        }

        if (ComplexType.GetChangeTrackingStrategyConfigurationSource() != null)
        {
            complexTypeBuilder.Metadata.SetChangeTrackingStrategy(
                ComplexType.GetChangeTrackingStrategy(), ComplexType.GetChangeTrackingStrategyConfigurationSource()!.Value);
        }

        Properties.Attach(complexTypeBuilder);

        if (ComplexType.GetConstructorBindingConfigurationSource() != null)
        {
            complexTypeBuilder.Metadata.SetConstructorBinding(
                Create(ComplexType.ConstructorBinding, complexTypeBuilder.Metadata),
                ComplexType.GetConstructorBindingConfigurationSource()!.Value);
        }

        if (ComplexType.GetServiceOnlyConstructorBindingConfigurationSource() != null)
        {
            complexTypeBuilder.Metadata.SetServiceOnlyConstructorBinding(
                Create(ComplexType.ServiceOnlyConstructorBinding, complexTypeBuilder.Metadata),
                ComplexType.GetServiceOnlyConstructorBindingConfigurationSource()!.Value);
        }

        return complexPropertyBuilder;
    }

    private static InstantiationBinding? Create(InstantiationBinding? instantiationBinding, ComplexType complexType)
        => instantiationBinding?.With(
            instantiationBinding.ParameterBindings.Select(binding => Create(binding, complexType)).ToList());

    private static ParameterBinding Create(ParameterBinding parameterBinding, ComplexType complexType)
        => parameterBinding.With(
            parameterBinding.ConsumedProperties.Select(
                property =>
                    (IPropertyBase?)complexType.FindProperty(property.Name)
                    ?? complexType.FindComplexProperty(property.Name)!).ToArray());
}
