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
        var sproc = StoredProcedure.GetDeclaredStoredProcedure(entityType, sprocType);
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
        var sproc = StoredProcedure.GetDeclaredStoredProcedure(entityType, sprocType);
        if (sproc == null)
        {
            sproc = StoredProcedure.SetStoredProcedure(entityType, sprocType, fromDataAnnotation);
        }
        else
        {
            sproc.UpdateConfigurationSource(fromDataAnnotation
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
    public virtual PropertyBuilder CreatePropertyBuilder(EntityTypeBuilder entityTypeBuilder, string propertyName)
    {
        var entityType = entityTypeBuilder.Metadata;
        var property = entityType.FindProperty(propertyName);
        if (property == null)
        {
            property = entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties())
                .FirstOrDefault(p => p.Name == propertyName);
        }

        if (property == null)
        {
            throw new InvalidOperationException(CoreStrings.PropertyNotFound(propertyName, entityType.DisplayName()));
        }

#pragma warning disable EF1001 // Internal EF Core API usage.
        return new ModelBuilder(entityType.Model)
#pragma warning restore EF1001 // Internal EF Core API usage.
            .Entity(property.DeclaringEntityType.Name)
            .Property(property.ClrType, propertyName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyBuilder CreatePropertyBuilder<TDerivedEntity, TProperty>(
        EntityTypeBuilder entityTypeBuilder,
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
        where TDerivedEntity : class
    {
        var memberInfo = propertyExpression.GetMemberAccess();
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.ClrType != typeof(TDerivedEntity))
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            entityTypeBuilder = new ModelBuilder(entityType.Model).Entity(typeof(TDerivedEntity));
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

        return entityTypeBuilder.Property(memberInfo.GetMemberType(), memberInfo.Name);
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasParameter(
        string propertyName, ConfigurationSource configurationSource)
    {
        if (!Metadata.ContainsParameter(propertyName))
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }
            
            Metadata.AddParameter(propertyName);
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return this;
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasParameter<TDerivedEntity, TProperty>(
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
        => Metadata.ContainsParameter(propertyName)
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasResultColumn(
        string propertyName, ConfigurationSource configurationSource)
    {
        if (!Metadata.ContainsResultColumn(propertyName))
        {
            if (!configurationSource.Overrides(Metadata.GetConfigurationSource()))
            {
                return null;
            }

            Metadata.AddResultColumn(propertyName);
        }

        Metadata.UpdateConfigurationSource(configurationSource);
        return this;
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? HasResultColumn<TDerivedEntity, TProperty>(
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
        => Metadata.ContainsResultColumn(propertyName)
            || configurationSource.Overrides(Metadata.GetConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalStoredProcedureBuilder? SuppressTransactions(bool suppress, ConfigurationSource configurationSource)
    {
        if (!CanSuppressTransactions(suppress, configurationSource))
        {
            return null;
        }

        Metadata.SetAreTransactionsSuppressed(suppress, configurationSource);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSuppressTransactions(bool suppress, ConfigurationSource configurationSource)
        => Metadata.AreTransactionsSuppressed == suppress
            || configurationSource.Overrides(Metadata.GetAreTransactionsSuppressedConfigurationSource());

    IConventionStoredProcedure IConventionStoredProcedureBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

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
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasParameter(string propertyName, bool fromDataAnnotation)
        => HasParameter(propertyName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveParameter(string propertyName, bool fromDataAnnotation)
        => CanHaveParameter(propertyName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.HasResultColumn(string propertyName, bool fromDataAnnotation)
        => HasResultColumn(propertyName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanHaveResultColumn(string propertyName, bool fromDataAnnotation)
        => CanHaveResultColumn(propertyName,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionStoredProcedureBuilder? IConventionStoredProcedureBuilder.SuppressTransactions(bool suppress, bool fromDataAnnotation)
        => SuppressTransactions(suppress, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionStoredProcedureBuilder.CanSetSuppressTransactions(bool suppress, bool fromDataAnnotation)
        => CanSuppressTransactions(suppress,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
