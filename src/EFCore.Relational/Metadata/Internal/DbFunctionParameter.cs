// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbFunctionParameter :
    ConventionAnnotatable,
    IMutableDbFunctionParameter,
    IConventionDbFunctionParameter,
    IRuntimeDbFunctionParameter
{
    private string? _storeType;
    private RelationalTypeMapping? _typeMapping;
    private bool _propagatesNullability;

    private ConfigurationSource? _storeTypeConfigurationSource;
    private ConfigurationSource? _typeMappingConfigurationSource;
    private ConfigurationSource? _propagatesNullabilityConfigurationSource;
    private InternalDbFunctionParameterBuilder? _builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DbFunctionParameter(
        DbFunction function,
        string name,
        Type clrType)
    {
        Name = name;
        Function = function;
        ClrType = clrType;
        _builder = new InternalDbFunctionParameterBuilder(this, function.Builder.ModelBuilder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalDbFunctionParameterBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(Name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
        => _builder = null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => ((Annotatable)Function.Model).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbFunction Function { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ConfigurationSource GetConfigurationSource()
        => Function.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoreFunctionParameter StoreFunctionParameter { get; set; } = default!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? StoreType
    {
        get => _storeType ?? TypeMapping?.StoreType;
        set => SetStoreType(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? SetStoreType(string? storeType, ConfigurationSource configurationSource)
    {
        _storeType = storeType;

        _storeTypeConfigurationSource = configurationSource.Max(_storeTypeConfigurationSource);

        return storeType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetStoreTypeConfigurationSource()
        => _storeTypeConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? TypeMapping
    {
        get => IsReadOnly
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _typeMapping, this, static parameter =>
                {
                    var relationalTypeMappingSource =
                        (IRelationalTypeMappingSource)((IModel)parameter.Function.Model).GetModelDependencies().TypeMappingSource;
                    return !string.IsNullOrEmpty(parameter._storeType)
                        ? relationalTypeMappingSource.FindMapping(parameter._storeType)!
                        : relationalTypeMappingSource.FindMapping(parameter.ClrType, (IModel)parameter.Function.Model)!;
                })
            : _typeMapping;
        set => SetTypeMapping(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? SetTypeMapping(
        RelationalTypeMapping? typeMapping,
        ConfigurationSource configurationSource)
    {
        _typeMapping = typeMapping;
        _typeMappingConfigurationSource = configurationSource.Max(_typeMappingConfigurationSource);

        return typeMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool PropagatesNullability
    {
        get => _propagatesNullability;
        set => SetPropagatesNullability(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SetPropagatesNullability(bool propagatesNullability, ConfigurationSource configurationSource)
    {
        if (!Function.IsScalar)
        {
            throw new InvalidOperationException(
                RelationalStrings.NonScalarFunctionParameterCannotPropagatesNullability(Name, Function.Name));
        }

        _propagatesNullability = propagatesNullability;
        _propagatesNullabilityConfigurationSource = configurationSource.Max(_storeTypeConfigurationSource);

        return propagatesNullability;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetPropagatesNullabilityConfigurationSource()
        => _propagatesNullabilityConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTypeMappingConfigurationSource()
        => _typeMappingConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IConventionDbFunctionParameterBuilder IConventionDbFunctionParameter.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <inheritdoc />
    IConventionDbFunction IConventionDbFunctionParameter.Function
    {
        [DebuggerStepThrough]
        get => Function;
    }

    /// <inheritdoc />
    IReadOnlyDbFunction IReadOnlyDbFunctionParameter.Function
    {
        [DebuggerStepThrough]
        get => Function;
    }

    /// <inheritdoc />
    IMutableDbFunction IMutableDbFunctionParameter.Function
    {
        [DebuggerStepThrough]
        get => Function;
    }

    /// <inheritdoc />
    IDbFunction IDbFunctionParameter.Function
    {
        [DebuggerStepThrough]
        get => Function;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    RelationalTypeMapping? IConventionDbFunctionParameter.SetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation)
        => SetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    string IDbFunctionParameter.StoreType
    {
        [DebuggerStepThrough]
        get => StoreType!;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IConventionDbFunctionParameter.SetStoreType(string? storeType, bool fromDataAnnotation)
        => SetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
