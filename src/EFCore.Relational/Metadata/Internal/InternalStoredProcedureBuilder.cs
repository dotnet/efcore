// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalStoredProcedureBuilder :
    AnnotatableBuilder<StoredProcedure, IConventionModelBuilder>,
    IConventionStoredProcedureBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalStoredProcedureBuilder(StoredProcedure storedProcedure, IConventionModelBuilder modelBuilder)
        : base(storedProcedure, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static InternalStoredProcedureBuilder HasStoredProcedure(
        IMutableEntityType entityType,
        StoreObjectType sprocType,
        string? name = null,
        string? schema = null)
    {
        var sproc = (StoredProcedure?)StoredProcedure.FindDeclaredStoredProcedure(entityType, sprocType);
        if (sproc == null)
        {
            sproc = name == null
                ? StoredProcedure.SetStoredProcedure(entityType, sprocType)
                : StoredProcedure.SetStoredProcedure(entityType, sprocType, name, schema);
        }
        else
        {
            if (name != null)
            {
                sproc.SetName(name, schema, ConfigurationSource.Explicit);
            }

            sproc.UpdateConfigurationSource(ConfigurationSource.Explicit);
        }

        return sproc.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static InternalStoredProcedureBuilder? HasStoredProcedure(
        IConventionEntityType entityType,
        StoreObjectType sprocType,
        bool fromDataAnnotation)
    {
        var sproc = (StoredProcedure?)StoredProcedure.FindDeclaredStoredProcedure(entityType, sprocType);
        if (sproc == null)
        {
            sproc = StoredProcedure.SetStoredProcedure(entityType, sprocType, fromDataAnnotation);
        }
        else
        {
            sproc.UpdateConfigurationSource(
                fromDataAnnotation
                    ? ConfigurationSource.DataAnnotation
                    : ConfigurationSource.Convention);
        }

        return sproc?.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasName(string? name, ConfigurationSource configurationSource)
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
    public virtual InternalStoredProcedureBuilder? HasName(string? name, string? schema, ConfigurationSource configurationSource)
    {
        if (CanSetName(name, configurationSource)
            && CanSetSchema(schema, configurationSource))
        {
            Metadata.SetName(name, schema, configurationSource);
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
    public virtual InternalStoredProcedureBuilder? HasSchema(string? schema, ConfigurationSource configurationSource)
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
    public virtual InternalStoredProcedureParameterBuilder? HasParameter(
        string propertyName,
        ConfigurationSource configurationSource)
    {
        var parameter = Metadata.FindParameter(propertyName);
        if (parameter == null)
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            parameter = Metadata.AddParameter(propertyName);
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return parameter.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder? HasParameter<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
        ConfigurationSource configurationSource)
        where TDerivedEntity : class
        => HasParameter(propertyExpression.GetMemberAccess().Name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveParameter(string propertyName, ConfigurationSource configurationSource)
        => Metadata.FindParameter(propertyName) != null
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder? HasOriginalValueParameter(
        string propertyName,
        ConfigurationSource configurationSource)
    {
        var parameter = Metadata.FindOriginalValueParameter(propertyName);
        if (parameter == null)
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            parameter = Metadata.AddOriginalValueParameter(propertyName);
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return parameter.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder? HasOriginalValueParameter<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
        ConfigurationSource configurationSource)
        where TDerivedEntity : class
        => HasOriginalValueParameter(propertyExpression.GetMemberAccess().Name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveOriginalValueParameter(string propertyName, ConfigurationSource configurationSource)
        => Metadata.FindOriginalValueParameter(propertyName) != null
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder? HasRowsAffectedParameter(
        ConfigurationSource configurationSource)
    {
        var parameter = Metadata.FindRowsAffectedParameter();
        if (parameter == null)
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            parameter = Metadata.AddRowsAffectedParameter();
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return parameter.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureParameterBuilder? HasRowsAffectedParameter<TDerivedEntity, TProperty>(
        ConfigurationSource configurationSource)
        where TDerivedEntity : class
        => HasRowsAffectedParameter(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveRowsAffectedParameter(ConfigurationSource configurationSource)
        => Metadata.FindRowsAffectedParameter() != null
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder? HasResultColumn(
        string propertyName,
        ConfigurationSource configurationSource)
    {
        var resultColumn = Metadata.FindResultColumn(propertyName);
        if (resultColumn == null)
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            resultColumn = Metadata.AddResultColumn(propertyName);
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return resultColumn.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder? HasResultColumn<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
        ConfigurationSource configurationSource)
        where TDerivedEntity : class
        => HasResultColumn(propertyExpression.GetMemberAccess().Name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveResultColumn(string propertyName, ConfigurationSource configurationSource)
        => Metadata.FindResultColumn(propertyName) != null
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder? HasRowsAffectedResultColumn(
        ConfigurationSource configurationSource)
    {
        var resultColumn = Metadata.FindRowsAffectedResultColumn();
        if (resultColumn == null)
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            resultColumn = Metadata.AddRowsAffectedResultColumn();
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return resultColumn.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureResultColumnBuilder? HasRowsAffectedResultColumn<TDerivedEntity, TProperty>(
        ConfigurationSource configurationSource)
        where TDerivedEntity : class
        => HasRowsAffectedResultColumn(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveRowsAffectedResultColumn(ConfigurationSource configurationSource)
        => Metadata.FindRowsAffectedResultColumn() != null
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasRowsAffectedReturn(bool rowsAffectedReturned, ConfigurationSource configurationSource)
    {
        if (!CanHaveRowsAffectedReturn(rowsAffectedReturned, configurationSource))
        {
            return null;
        }

        Metadata.SetIsRowsAffectedReturned(rowsAffectedReturned);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanHaveRowsAffectedReturn(bool rowsAffectedReturned, ConfigurationSource configurationSource)
        => Metadata.IsRowsAffectedReturned == rowsAffectedReturned
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    IConventionStoredProcedure IConventionStoredProcedureBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => (IConventionStoredProcedureBuilder?)base.HasAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
        => (IConventionStoredProcedureBuilder?)base.HasNonNullAnnotation(
            name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => (IConventionStoredProcedureBuilder?)base.HasNoAnnotation(
            name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasName(string? name, bool fromDataAnnotation)
        => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasName(string? name, string? schema, bool fromDataAnnotation)
        => HasName(name, schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanSetName(string? name, bool fromDataAnnotation)
        => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasSchema(string? schema, bool fromDataAnnotation)
        => HasSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanSetSchema(string? schema, bool fromDataAnnotation)
        => CanSetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameterBuilder? IConventionStoredProcedureBuilder.HasParameter(string propertyName, bool fromDataAnnotation)
        => HasParameter(propertyName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveParameter(string propertyName, bool fromDataAnnotation)
        => CanHaveParameter(
            propertyName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameterBuilder? IConventionStoredProcedureBuilder.HasOriginalValueParameter(
        string propertyName,
        bool fromDataAnnotation)
        => HasOriginalValueParameter(
            propertyName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveOriginalValueParameter(string propertyName, bool fromDataAnnotation)
        => CanHaveOriginalValueParameter(
            propertyName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureParameterBuilder? IConventionStoredProcedureBuilder.HasRowsAffectedParameter(bool fromDataAnnotation)
        => HasRowsAffectedParameter(
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveRowsAffectedParameter(bool fromDataAnnotation)
        => CanHaveRowsAffectedParameter(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureBuilder.HasResultColumn(
        string propertyName,
        bool fromDataAnnotation)
        => HasResultColumn(propertyName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveResultColumn(string propertyName, bool fromDataAnnotation)
        => CanHaveResultColumn(
            propertyName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureResultColumnBuilder? IConventionStoredProcedureBuilder.HasRowsAffectedResultColumn(bool fromDataAnnotation)
        => HasRowsAffectedResultColumn(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveRowsAffectedResultColumn(bool fromDataAnnotation)
        => CanHaveRowsAffectedResultColumn(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
