// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalDbFunctionBuilder : AnnotatableBuilder<DbFunction, IConventionModelBuilder>, IConventionDbFunctionBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalDbFunctionBuilder(DbFunction function, IConventionModelBuilder modelBuilder)
        : base(function, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? HasName(string? name, ConfigurationSource configurationSource)
    {
        if (CanSetName(name, configurationSource))
        {
            Metadata.SetName(name, configurationSource);
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
    public virtual bool CanSetName(string? name, ConfigurationSource configurationSource)
        => (name != "" || configurationSource == ConfigurationSource.Explicit)
            && (configurationSource.Overrides(Metadata.GetNameConfigurationSource())
                || Metadata.Name == name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? HasSchema(string? schema, ConfigurationSource configurationSource)
    {
        if (CanSetSchema(schema, configurationSource))
        {
            Metadata.SetSchema(schema, configurationSource);
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
    public virtual bool CanSetSchema(string? schema, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetSchemaConfigurationSource())
            || Metadata.Schema == schema;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? IsBuiltIn(bool builtIn, ConfigurationSource configurationSource)
    {
        if (CanSetIsBuiltIn(builtIn, configurationSource))
        {
            Metadata.SetIsBuiltIn(builtIn, configurationSource);
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
    public virtual bool CanSetIsBuiltIn(bool builtIn, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetIsBuiltInConfigurationSource())
            || Metadata.IsBuiltIn == builtIn;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? IsNullable(bool nullable, ConfigurationSource configurationSource)
    {
        if (CanSetIsNullable(nullable, configurationSource))
        {
            Metadata.SetIsNullable(nullable, configurationSource);
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
    public virtual bool CanSetIsNullable(bool nullable, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetIsNullableConfigurationSource())
            || Metadata.IsNullable == nullable;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? HasStoreType(string? storeType, ConfigurationSource configurationSource)
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
    public virtual IConventionDbFunctionBuilder? HasTypeMapping(
        RelationalTypeMapping? returnTypeMapping,
        ConfigurationSource configurationSource)
    {
        if (CanSetTypeMapping(returnTypeMapping, configurationSource))
        {
            Metadata.SetTypeMapping(returnTypeMapping, configurationSource);
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
    public virtual bool CanSetTypeMapping(RelationalTypeMapping? returnTypeMapping, ConfigurationSource configurationSource)
        => configurationSource.Overrides(Metadata.GetTypeMappingConfigurationSource())
            || Metadata.TypeMapping == returnTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IConventionDbFunctionBuilder? HasTranslation(
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
        ConfigurationSource configurationSource)
    {
        if (CanSetTranslation(translation, configurationSource))
        {
            Metadata.SetTranslation(translation, configurationSource);
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
    public virtual bool CanSetTranslation(
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
        ConfigurationSource configurationSource)
        => (Metadata is { IsScalar: true, IsAggregate: false } || configurationSource == ConfigurationSource.Explicit)
            && (configurationSource.Overrides(Metadata.GetTranslationConfigurationSource())
                || Metadata.Translation == translation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalDbFunctionParameterBuilder HasParameter(string name, ConfigurationSource configurationSource)
    {
        var parameter = Metadata.FindParameter(name);
        if (parameter == null)
        {
            throw new ArgumentException(
                RelationalStrings.DbFunctionInvalidParameterName(Metadata.MethodInfo?.DisplayName(), name));
        }

        return parameter.Builder;
    }

    IConventionDbFunction IConventionDbFunctionBuilder.Metadata
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
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionDbFunctionBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionDbFunctionBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionDbFunctionBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasName(string? name, bool fromDataAnnotation)
        => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetName(string? name, bool fromDataAnnotation)
        => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasSchema(string? schema, bool fromDataAnnotation)
        => HasSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetSchema(string? schema, bool fromDataAnnotation)
        => CanSetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.IsBuiltIn(bool builtIn, bool fromDataAnnotation)
        => IsBuiltIn(builtIn, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetIsBuiltIn(bool builtIn, bool fromDataAnnotation)
        => CanSetIsBuiltIn(builtIn, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.IsNullable(bool nullable, bool fromDataAnnotation)
        => IsNullable(nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetIsNullable(bool nullable, bool fromDataAnnotation)
        => CanSetIsNullable(nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasStoreType(string? storeType, bool fromDataAnnotation)
        => HasStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetStoreType(string? storeType, bool fromDataAnnotation)
        => CanSetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasTypeMapping(
        RelationalTypeMapping? typeMapping,
        bool fromDataAnnotation)
        => HasTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation)
        => CanSetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionBuilder? IConventionDbFunctionBuilder.HasTranslation(
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
        bool fromDataAnnotation)
        => HasTranslation(translation, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDbFunctionBuilder.CanSetTranslation(
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
        bool fromDataAnnotation)
        => CanSetTranslation(translation, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDbFunctionParameterBuilder IConventionDbFunctionBuilder.HasParameter(string name, bool fromDataAnnotation)
        => HasParameter(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
