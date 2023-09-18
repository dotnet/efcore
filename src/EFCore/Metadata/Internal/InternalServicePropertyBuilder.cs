// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalServicePropertyBuilder :
    InternalPropertyBaseBuilder<IConventionServicePropertyBuilder, ServiceProperty>,
    IConventionServicePropertyBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalServicePropertyBuilder(ServiceProperty property, InternalModelBuilder modelBuilder)
        : base(property, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IConventionServicePropertyBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalServicePropertyBuilder? HasField(string? fieldName, ConfigurationSource configurationSource)
        => (InternalServicePropertyBuilder?)base.HasField(fieldName, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalServicePropertyBuilder? HasField(
        FieldInfo? fieldInfo,
        ConfigurationSource configurationSource)
        => (InternalServicePropertyBuilder?)base.HasField(fieldInfo, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalServicePropertyBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => (InternalServicePropertyBuilder?)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalServicePropertyBuilder? HasParameterBinding(
        ServiceParameterBinding? parameterBinding,
        ConfigurationSource configurationSource)
    {
        if (CanSetParameterBinding(parameterBinding, configurationSource))
        {
            Metadata.SetParameterBinding(parameterBinding, configurationSource);
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
    public virtual bool CanSetParameterBinding(
        ServiceParameterBinding? parameterBinding,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetParameterBindingConfigurationSource())
            || (Metadata.ParameterBinding == parameterBinding);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalServicePropertyBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var newPropertyBuilder = entityTypeBuilder.ServiceProperty(
            Metadata.ClrType, Metadata.GetIdentifyingMemberInfo()!, Metadata.GetConfigurationSource());
        if (newPropertyBuilder == null)
        {
            return null;
        }

        newPropertyBuilder.MergeAnnotationsFrom(Metadata);

        var oldParameterBindingConfigurationSource = Metadata.GetParameterBindingConfigurationSource();
        if (oldParameterBindingConfigurationSource.HasValue)
        {
            newPropertyBuilder.HasParameterBinding(Metadata.ParameterBinding, oldParameterBindingConfigurationSource.Value);
        }

        var oldFieldInfoConfigurationSource = Metadata.GetFieldInfoConfigurationSource();
        if (oldFieldInfoConfigurationSource.HasValue
            && newPropertyBuilder.CanSetField(Metadata.FieldInfo, oldFieldInfoConfigurationSource))
        {
            newPropertyBuilder.HasField(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
        }

        var propertyAccessModeConfigurationSource = Metadata.GetPropertyAccessModeConfigurationSource();
        if (propertyAccessModeConfigurationSource.HasValue)
        {
            newPropertyBuilder.UsePropertyAccessMode(
                Metadata.GetPropertyAccessMode(), propertyAccessModeConfigurationSource.Value);
        }

        return newPropertyBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionServiceProperty IConventionServicePropertyBuilder.Metadata
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
    IConventionPropertyBase IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.Metadata
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
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionServicePropertyBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionServicePropertyBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
        => (IConventionServicePropertyBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.HasField(
        string? fieldName,
        bool fromDataAnnotation)
        => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.HasField(
        FieldInfo? fieldInfo,
        bool fromDataAnnotation)
        => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.CanSetField(string? fieldName, bool fromDataAnnotation)
        => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionServicePropertyBuilder? IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.UsePropertyAccessMode(
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
    bool IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>.CanSetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionServicePropertyBuilder? IConventionServicePropertyBuilder.HasParameterBinding(
        ServiceParameterBinding? parameterBinding,
        bool fromDataAnnotation)
        => HasParameterBinding(
            parameterBinding, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionServicePropertyBuilder.CanSetParameterBinding(ServiceParameterBinding? parameterBinding, bool fromDataAnnotation)
        => CanSetParameterBinding(
            parameterBinding, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
