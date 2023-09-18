// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     <para>
///         Provides a simple API for configuring a <see cref="DbFunctionParameter" />.
///     </para>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
/// </remarks>
public class InternalDbFunctionParameterBuilder : AnnotatableBuilder<DbFunctionParameter, IConventionModelBuilder>,
    IConventionDbFunctionParameterBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public InternalDbFunctionParameterBuilder(DbFunctionParameter parameter, IConventionModelBuilder modelBuilder)
        : base(parameter, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionParameterBuilder? HasStoreType(
        string? storeType,
        ConfigurationSource configurationSource)
    {
        if (CanSetStoreType(storeType, configurationSource))
        {
            Metadata.SetStoreType(storeType, configurationSource);
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
    public virtual bool CanSetStoreType(string? storeType, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetStoreTypeConfigurationSource())
            || Metadata.StoreType == storeType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionParameterBuilder? HasTypeMapping(
        RelationalTypeMapping? typeMapping,
        ConfigurationSource configurationSource)
    {
        if (CanSetTypeMapping(typeMapping, configurationSource))
        {
            Metadata.SetTypeMapping(typeMapping, configurationSource);
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
    public virtual bool CanSetTypeMapping(RelationalTypeMapping? typeMapping, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetTypeMappingConfigurationSource())
            || Metadata.TypeMapping == typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionParameterBuilder? PropagatesNullability(
        bool propagatesNullability,
        ConfigurationSource configurationSource)
    {
        if (CanSetPropagatesNullability(propagatesNullability, configurationSource))
        {
            Metadata.SetPropagatesNullability(propagatesNullability, configurationSource);
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
    public virtual bool CanSetPropagatesNullability(bool propagatesNullability, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetPropagatesNullabilityConfigurationSource())
            || Metadata.PropagatesNullability == propagatesNullability;

    /// <inheritdoc />
    IConventionDbFunctionParameter IConventionDbFunctionParameterBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder? IConventionDbFunctionParameterBuilder.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionDbFunctionParameterBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder? IConventionDbFunctionParameterBuilder.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionDbFunctionParameterBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder? IConventionDbFunctionParameterBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionDbFunctionParameterBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder? IConventionDbFunctionParameterBuilder.HasStoreType(
        string? storeType,
        bool fromDataAnnotation)
        => HasStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionParameterBuilder.CanSetStoreType(string? storeType, bool fromDataAnnotation)
        => CanSetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder? IConventionDbFunctionParameterBuilder.HasTypeMapping(
        RelationalTypeMapping? typeMapping,
        bool fromDataAnnotation)
        => HasTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionParameterBuilder.CanSetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation)
        => CanSetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
