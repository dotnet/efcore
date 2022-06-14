// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Entity type extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalEntityTypeExtensions
{
    /// <summary>
    ///     Gets the name used for the <see cref="ISqlQuery" /> mapped using
    ///     <see cref="O:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
    /// </summary>
    public static readonly string DefaultQueryNameBase = "MappedSqlQuery";

    #region Table mapping

    /// <summary>
    ///     Returns the name of the table to which the entity type is mapped
    ///     or <see langword="null" /> if not mapped to a table.
    /// </summary>
    /// <param name="entityType">The entity type to get the table name for.</param>
    /// <returns>The name of the table to which the entity type is mapped.</returns>
    public static string? GetTableName(this IReadOnlyEntityType entityType)
    {
        var nameAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.TableName);
        if (nameAnnotation != null)
        {
            return (string?)nameAnnotation.Value;
        }

        return ((entityType as IConventionEntityType)?.GetViewNameConfigurationSource() == null)
            && (entityType as IConventionEntityType)?.GetFunctionNameConfigurationSource() == null
#pragma warning disable CS0618 // Type or member is obsolete
            && (entityType as IConventionEntityType)?.GetDefiningQueryConfigurationSource() == null
#pragma warning restore CS0618 // Type or member is obsolete
            && (entityType as IConventionEntityType)?.GetSqlQueryConfigurationSource() == null
                ? GetDefaultTableName(entityType)
                : null;
    }

    /// <summary>
    ///     Returns the default table name that would be used for this entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the table name for.</param>
    /// <param name="truncate">A value indicating whether the name should be truncated to the max identifier length.</param>
    /// <returns>The default name of the table to which the entity type would be mapped.</returns>
    public static string? GetDefaultTableName(this IReadOnlyEntityType entityType, bool truncate = true)
    {
        if (entityType.GetDiscriminatorPropertyName() != null
            && entityType.BaseType != null)
        {
            return entityType.GetRootType().GetTableName();
        }

        var ownership = entityType.FindOwnership();
        if (ownership != null
            && ownership.IsUnique)
        {
            return ownership.PrincipalEntityType.GetTableName();
        }

        var name = entityType.ShortName();
        if (entityType.HasSharedClrType
            && ownership != null
#pragma warning disable EF1001 // Internal EF Core API usage.
            && entityType.Name == ownership.PrincipalEntityType.GetOwnedName(name, ownership.PrincipalToDependent!.Name))
#pragma warning restore EF1001 // Internal EF Core API usage.
        {
            var ownerTypeTable = ownership.PrincipalEntityType.GetTableName();
            name = ownerTypeTable != null
                ? $"{ownerTypeTable}_{ownership.PrincipalToDependent.Name}"
                : $"{ownership.PrincipalToDependent.Name}_{name}";
        }

        if (entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
            && !entityType.ClrType.IsInstantiable())
        {
            return null;
        }

        return truncate
            ? Uniquifier.Truncate(name, entityType.Model.GetMaxIdentifierLength())
            : name;
    }

    /// <summary>
    ///     Sets the name of the table to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the table name for.</param>
    /// <param name="name">The name to set.</param>
    public static void SetTableName(this IMutableEntityType entityType, string? name)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.TableName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name of the table to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the table name for.</param>
    /// <param name="name">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured table name.</returns>
    public static string? SetTableName(
        this IConventionEntityType entityType,
        string? name,
        bool fromDataAnnotation = false)
    {
        entityType.SetAnnotation(
            RelationalAnnotationNames.TableName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation);

        return name;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the table name.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the table name.</returns>
    public static ConfigurationSource? GetTableNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.TableName)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the database schema that contains the mapped table.
    /// </summary>
    /// <param name="entityType">The entity type to get the schema for.</param>
    /// <returns>The database schema that contains the mapped table.</returns>
    public static string? GetSchema(this IReadOnlyEntityType entityType)
    {
        if (entityType.GetTableName() == null)
        {
            return null;
        }

        var schemaAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.Schema);
        if (schemaAnnotation != null)
        {
            return (string?)schemaAnnotation.Value ?? GetDefaultSchema(entityType);
        }

        return entityType.BaseType != null
            ? entityType.GetRootType().GetSchema()
            : GetDefaultSchema(entityType);
    }

    /// <summary>
    ///     Returns the default database schema that would be used for this entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the schema for.</param>
    /// <returns>The default database schema to which the entity type would be mapped.</returns>
    public static string? GetDefaultSchema(this IReadOnlyEntityType entityType)
    {
        var ownership = entityType.FindOwnership();
        if (ownership != null)
        {
            return ownership.PrincipalEntityType.GetSchema();
        }

        var skipNavigationSchema = entityType.GetForeignKeys().SelectMany(fk => fk.GetReferencingSkipNavigations())
            .FirstOrDefault(n => !n.IsOnDependent)
            ?.DeclaringEntityType.GetSchema();
        if (skipNavigationSchema != null
            && entityType.GetForeignKeys().SelectMany(fk => fk.GetReferencingSkipNavigations())
                .Where(n => !n.IsOnDependent)
                .All(n => n.DeclaringEntityType.GetSchema() == skipNavigationSchema))
        {
            return skipNavigationSchema;
        }

        return entityType.Model.GetDefaultSchema();
    }

    /// <summary>
    ///     Sets the database schema that contains the mapped table.
    /// </summary>
    /// <param name="entityType">The entity type to set the schema for.</param>
    /// <param name="value">The value to set.</param>
    public static void SetSchema(this IMutableEntityType entityType, string? value)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.Schema,
            Check.NullButNotEmpty(value, nameof(value)));

    /// <summary>
    ///     Sets the database schema that contains the mapped table.
    /// </summary>
    /// <param name="entityType">The entity type to set the schema for.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSchema(
        this IConventionEntityType entityType,
        string? value,
        bool fromDataAnnotation = false)
    {
        entityType.SetAnnotation(
            RelationalAnnotationNames.Schema,
            Check.NullButNotEmpty(value, nameof(value)),
            fromDataAnnotation);

        return value;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the database schema.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the database schema.</returns>
    public static ConfigurationSource? GetSchemaConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.Schema)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the name of the table to which the entity type is mapped prepended by the schema
    ///     or <see langword="null" /> if not mapped to a table.
    /// </summary>
    /// <param name="entityType">The entity type to get the table name for.</param>
    /// <returns>The name of the table to which the entity type is mapped prepended by the schema.</returns>
    public static string? GetSchemaQualifiedTableName(this IReadOnlyEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        if (tableName == null)
        {
            return null;
        }

        var schema = entityType.GetSchema();
        return (string.IsNullOrEmpty(schema) ? "" : schema + ".") + tableName;
    }

    /// <summary>
    ///     Returns the name of the view to which the entity type is mapped prepended by the schema
    ///     or <see langword="null" /> if not mapped to a view.
    /// </summary>
    /// <param name="entityType">The entity type to get the table name for.</param>
    /// <returns>The name of the table to which the entity type is mapped prepended by the schema.</returns>
    public static string? GetSchemaQualifiedViewName(this IReadOnlyEntityType entityType)
    {
        var viewName = entityType.GetViewName();
        if (viewName == null)
        {
            return null;
        }

        var schema = entityType.GetViewSchema();
        return (string.IsNullOrEmpty(schema) ? "" : schema + ".") + viewName;
    }

    /// <summary>
    ///     Returns the default mappings that the entity type would use.
    /// </summary>
    /// <param name="entityType">The entity type to get the table mappings for.</param>
    /// <returns>The tables to which the entity type is mapped.</returns>
    public static IEnumerable<ITableMappingBase> GetDefaultMappings(this IEntityType entityType)
        => (IEnumerable<ITableMappingBase>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DefaultMappings)
            ?? Enumerable.Empty<ITableMappingBase>();

    /// <summary>
    ///     Returns the tables to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to get the table mappings for.</param>
    /// <returns>The tables to which the entity type is mapped.</returns>
    public static IEnumerable<ITableMapping> GetTableMappings(this IEntityType entityType)
        => (IEnumerable<ITableMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableMappings)
            ?? Enumerable.Empty<ITableMapping>();

    #endregion Table mapping

    #region View mapping

    /// <summary>
    ///     Returns the name of the view to which the entity type is mapped or <see langword="null" /> if not mapped to a view.
    /// </summary>
    /// <param name="entityType">The entity type to get the view name for.</param>
    /// <returns>The name of the view to which the entity type is mapped.</returns>
    public static string? GetViewName(this IReadOnlyEntityType entityType)
    {
        var nameAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.ViewName);
        if (nameAnnotation != null)
        {
            return (string?)nameAnnotation.Value;
        }

        return ((entityType as IConventionEntityType)?.GetFunctionNameConfigurationSource() == null)
#pragma warning disable CS0618 // Type or member is obsolete
            && ((entityType as IConventionEntityType)?.GetDefiningQueryConfigurationSource() == null)
#pragma warning restore CS0618 // Type or member is obsolete
            && ((entityType as IConventionEntityType)?.GetSqlQueryConfigurationSource() == null)
                ? GetDefaultViewName(entityType)
                : null;
    }

    /// <summary>
    ///     Returns the default view name that would be used for this entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the table name for.</param>
    /// <returns>The default name of the table to which the entity type would be mapped.</returns>
    public static string? GetDefaultViewName(this IReadOnlyEntityType entityType)
    {
        if (entityType.GetDiscriminatorPropertyName() != null
            && entityType.BaseType != null)
        {
            return entityType.GetRootType().GetViewName();
        }

        var ownership = entityType.FindOwnership();
        return ownership != null
            && ownership.IsUnique
                ? ownership.PrincipalEntityType.GetViewName()
                : null;
    }

    /// <summary>
    ///     Sets the name of the view to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the view name for.</param>
    /// <param name="name">The name to set.</param>
    public static void SetViewName(this IMutableEntityType entityType, string? name)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.ViewName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name of the view to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the view name for.</param>
    /// <param name="name">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetViewName(
        this IConventionEntityType entityType,
        string? name,
        bool fromDataAnnotation = false)
    {
        entityType.SetAnnotation(
            RelationalAnnotationNames.ViewName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation);

        return name;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the view name.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the view name.</returns>
    public static ConfigurationSource? GetViewNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.ViewName)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the database schema that contains the mapped view.
    /// </summary>
    /// <param name="entityType">The entity type to get the view schema for.</param>
    /// <returns>The database schema that contains the mapped view.</returns>
    public static string? GetViewSchema(this IReadOnlyEntityType entityType)
    {
        var schemaAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.ViewSchema);
        if (schemaAnnotation != null)
        {
            return (string?)schemaAnnotation.Value ?? GetDefaultViewSchema(entityType);
        }

        return entityType.BaseType != null
            ? entityType.GetRootType().GetViewSchema()
            : GetDefaultViewSchema(entityType);
    }

    /// <summary>
    ///     Returns the default database schema that would be used for this entity view.
    /// </summary>
    /// <param name="entityType">The entity type to get the view schema for.</param>
    /// <returns>The default database schema to which the entity type would be mapped.</returns>
    public static string? GetDefaultViewSchema(this IReadOnlyEntityType entityType)
    {
        var ownership = entityType.FindOwnership();
        if (ownership != null
            && ownership.IsUnique)
        {
            return ownership.PrincipalEntityType.GetViewSchema();
        }

        return GetViewName(entityType) != null ? entityType.Model.GetDefaultSchema() : null;
    }

    /// <summary>
    ///     Sets the database schema that contains the mapped view.
    /// </summary>
    /// <param name="entityType">The entity type to set the view schema for.</param>
    /// <param name="value">The value to set.</param>
    public static void SetViewSchema(this IMutableEntityType entityType, string? value)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.ViewSchema,
            Check.NullButNotEmpty(value, nameof(value)));

    /// <summary>
    ///     Sets the database schema that contains the mapped view.
    /// </summary>
    /// <param name="entityType">The entity type to set the view schema for.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured schema.</returns>
    public static string? SetViewSchema(
        this IConventionEntityType entityType,
        string? value,
        bool fromDataAnnotation = false)
    {
        entityType.SetAnnotation(
            RelationalAnnotationNames.ViewSchema,
            Check.NullButNotEmpty(value, nameof(value)),
            fromDataAnnotation);

        return value;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the view schema.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the view schema.</returns>
    public static ConfigurationSource? GetViewSchemaConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.ViewSchema)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the views to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to get the view mappings for.</param>
    /// <returns>The views to which the entity type is mapped.</returns>
    public static IEnumerable<IViewMapping> GetViewMappings(this IEntityType entityType)
        => (IEnumerable<IViewMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewMappings)
            ?? Enumerable.Empty<IViewMapping>();

    #endregion View mapping

    #region SQL query mapping

    /// <summary>
    ///     Gets the default SQL query name that would be used for this entity type when mapped using
    ///     <see cref="O:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Gets the default SQL query name.</returns>
    public static string GetDefaultSqlQueryName(this IReadOnlyEntityType entityType)
        => entityType.Name + "." + DefaultQueryNameBase;

    /// <summary>
    ///     Returns the SQL string used to provide data for the entity type or <see langword="null" /> if not mapped to a SQL string.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The SQL string used to provide data for the entity type.</returns>
    public static string? GetSqlQuery(this IReadOnlyEntityType entityType)
        => (string?)entityType[RelationalAnnotationNames.SqlQuery]
            ?? (entityType.BaseType != null
                ? entityType.GetRootType().GetSqlQuery()
                : null);

    /// <summary>
    ///     Sets the SQL string used to provide data for the entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="name">The SQL string to set.</param>
    public static void SetSqlQuery(this IMutableEntityType entityType, string? name)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.SqlQuery,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the SQL string used to provide data for the entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="name">The SQL string to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSqlQuery(
        this IConventionEntityType entityType,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            RelationalAnnotationNames.SqlQuery,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the query SQL string.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the query SQL string.</returns>
    public static ConfigurationSource? GetSqlQueryConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.SqlQuery)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the SQL string mappings.
    /// </summary>
    /// <param name="entityType">The entity type to get the function mappings for.</param>
    /// <returns>The functions to which the entity type is mapped.</returns>
    public static IEnumerable<ISqlQueryMapping> GetSqlQueryMappings(this IEntityType entityType)
        => (IEnumerable<ISqlQueryMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.SqlQueryMappings)
            ?? Enumerable.Empty<ISqlQueryMapping>();

    #endregion SQL query mapping

    #region Function mapping

    /// <summary>
    ///     Returns the name of the function to which the entity type is mapped or <see langword="null" /> if not mapped to a function.
    /// </summary>
    /// <param name="entityType">The entity type to get the function name for.</param>
    /// <returns>The name of the function to which the entity type is mapped.</returns>
    public static string? GetFunctionName(this IReadOnlyEntityType entityType)
        => (string?)entityType[RelationalAnnotationNames.FunctionName]
            ?? (entityType.BaseType != null
                ? entityType.GetRootType().GetFunctionName()
                : null);

    /// <summary>
    ///     Sets the name of the function to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the function name for.</param>
    /// <param name="name">The name to set.</param>
    public static void SetFunctionName(this IMutableEntityType entityType, string? name)
        => entityType.SetAnnotation(
            RelationalAnnotationNames.FunctionName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name of the function to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to set the function name for.</param>
    /// <param name="name">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetFunctionName(
        this IConventionEntityType entityType,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            RelationalAnnotationNames.FunctionName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the function name.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the function name.</returns>
    public static ConfigurationSource? GetFunctionNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.FunctionName)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Returns the functions to which the entity type is mapped.
    /// </summary>
    /// <param name="entityType">The entity type to get the function mappings for.</param>
    /// <returns>The functions to which the entity type is mapped.</returns>
    public static IEnumerable<IFunctionMapping> GetFunctionMappings(this IEntityType entityType)
        => (IEnumerable<IFunctionMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.FunctionMappings)
            ?? Enumerable.Empty<IFunctionMapping>();

    #endregion Function mapping

    #region Check constraint

    /// <summary>
    ///     Finds an <see cref="IReadOnlyCheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the check constraint for.</param>
    /// <param name="name">The check constraint name.</param>
    /// <returns>
    ///     The <see cref="IReadOnlyCheckConstraint" /> or <see langword="null" /> if no check constraint with the
    ///     given name in the given entity type was found.
    /// </returns>
    public static IReadOnlyCheckConstraint? FindCheckConstraint(
        this IReadOnlyEntityType entityType,
        string name)
    {
        Check.NotEmpty(name, nameof(name));

        return CheckConstraint.FindCheckConstraint(entityType, name);
    }

    /// <summary>
    ///     Finds an <see cref="IMutableCheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the check constraint for.</param>
    /// <param name="name">The check constraint name.</param>
    /// <returns>
    ///     The <see cref="IMutableCheckConstraint" /> or <see langword="null" /> if no check constraint with the
    ///     given name in the given entity type was found.
    /// </returns>
    public static IMutableCheckConstraint? FindCheckConstraint(
        this IMutableEntityType entityType,
        string name)
        => (IMutableCheckConstraint?)((IReadOnlyEntityType)entityType).FindCheckConstraint(name);

    /// <summary>
    ///     Finds an <see cref="IConventionCheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the check constraint for.</param>
    /// <param name="name">The check constraint name.</param>
    /// <returns>
    ///     The <see cref="IConventionCheckConstraint" /> or <see langword="null" /> if no check constraint with the
    ///     given name in the given entity type was found.
    /// </returns>
    public static IConventionCheckConstraint? FindCheckConstraint(
        this IConventionEntityType entityType,
        string name)
        => (IConventionCheckConstraint?)((IReadOnlyEntityType)entityType).FindCheckConstraint(name);

    /// <summary>
    ///     Finds an <see cref="ICheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the check constraint for.</param>
    /// <param name="name">The check constraint name.</param>
    /// <returns>
    ///     The <see cref="ICheckConstraint" /> or <see langword="null" /> if no check constraint with the
    ///     given name in the given entity type was found.
    /// </returns>
    public static ICheckConstraint? FindCheckConstraint(
        this IEntityType entityType,
        string name)
        => (ICheckConstraint?)((IReadOnlyEntityType)entityType).FindCheckConstraint(name);

    /// <summary>
    ///     Creates a new check constraint with the given name on entity type. Throws an exception
    ///     if a check constraint with the same name exists on the same entity type.
    /// </summary>
    /// <param name="entityType">The entity type to add the check constraint to.</param>
    /// <param name="name">The check constraint name.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>The new check constraint.</returns>
    public static IMutableCheckConstraint AddCheckConstraint(
        this IMutableEntityType entityType,
        string name,
        string sql)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(sql, nameof(sql));

        return new CheckConstraint(entityType, name, sql, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     Creates a new check constraint with the given name on entity type. Throws an exception
    ///     if a check constraint with the same name exists on the same entity type.
    /// </summary>
    /// <param name="entityType">The entity type to add the check constraint to.</param>
    /// <param name="name">The check constraint name.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new check constraint.</returns>
    public static IConventionCheckConstraint AddCheckConstraint(
        this IConventionEntityType entityType,
        string name,
        string sql,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(sql, nameof(sql));

        return new CheckConstraint(
            (IMutableEntityType)entityType, name, sql,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }

    /// <summary>
    ///     Removes the <see cref="ICheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to remove the check constraint from.</param>
    /// <param name="name">The check constraint name to be removed.</param>
    /// <returns>The removed check constraint.</returns>
    public static IMutableCheckConstraint? RemoveCheckConstraint(
        this IMutableEntityType entityType,
        string name)
        => CheckConstraint.RemoveCheckConstraint(entityType, Check.NotEmpty(name, nameof(name)));

    /// <summary>
    ///     Removes the <see cref="IConventionCheckConstraint" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to remove the check constraint from.</param>
    /// <param name="name">The check constraint name.</param>
    /// <returns>The removed check constraint.</returns>
    public static IConventionCheckConstraint? RemoveCheckConstraint(
        this IConventionEntityType entityType,
        string name)
        => (IConventionCheckConstraint?)CheckConstraint.RemoveCheckConstraint(
            (IMutableEntityType)entityType, Check.NotEmpty(name, nameof(name)));

    /// <summary>
    ///     Returns all check constraints contained in the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IReadOnlyCheckConstraint> GetCheckConstraints(this IReadOnlyEntityType entityType)
        => CheckConstraint.GetCheckConstraints(entityType);

    /// <summary>
    ///     Returns all check constraints contained in the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IMutableCheckConstraint> GetCheckConstraints(this IMutableEntityType entityType)
        => CheckConstraint.GetCheckConstraints(entityType).Cast<IMutableCheckConstraint>();

    /// <summary>
    ///     Returns all check constraints contained in the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IConventionCheckConstraint> GetCheckConstraints(this IConventionEntityType entityType)
        => CheckConstraint.GetCheckConstraints(entityType).Cast<IConventionCheckConstraint>();

    /// <summary>
    ///     Returns all check constraints contained in the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<ICheckConstraint> GetCheckConstraints(this IEntityType entityType)
        => CheckConstraint.GetCheckConstraints(entityType).Cast<ICheckConstraint>();

    /// <summary>
    ///     Returns all check constraints declared on the entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return check constraints declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
    ///     Use <see cref="GetCheckConstraints(IReadOnlyEntityType)" /> to also return check constraints declared on base types.
    /// </remarks>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IReadOnlyCheckConstraint> GetDeclaredCheckConstraints(this IReadOnlyEntityType entityType)
        => CheckConstraint.GetDeclaredCheckConstraints(entityType);

    /// <summary>
    ///     Returns all check constraints declared on the entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return check constraints declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
    ///     Use <see cref="GetCheckConstraints(IMutableEntityType)" /> to also return check constraints declared on base types.
    /// </remarks>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IMutableCheckConstraint> GetDeclaredCheckConstraints(this IMutableEntityType entityType)
        => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<IMutableCheckConstraint>();

    /// <summary>
    ///     Returns all check constraints declared on the entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return check constraints declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
    ///     Use <see cref="GetCheckConstraints(IConventionEntityType)" /> to also return check constraints declared on base types.
    /// </remarks>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<IConventionCheckConstraint> GetDeclaredCheckConstraints(this IConventionEntityType entityType)
        => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<IConventionCheckConstraint>();

    /// <summary>
    ///     Returns all check constraints declared on the entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return check constraints declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
    ///     Use <see cref="GetCheckConstraints(IEntityType)" /> to also return check constraints declared on base types.
    /// </remarks>
    /// <param name="entityType">The entity type to get the check constraints for.</param>
    public static IEnumerable<ICheckConstraint> GetDeclaredCheckConstraints(this IEntityType entityType)
        => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<ICheckConstraint>();

    #endregion Check constraint

    #region Comment

    /// <summary>
    ///     Returns the comment for the table this entity is mapped to.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The comment for the table this entity is mapped to.</returns>
    public static string? GetComment(this IReadOnlyEntityType entityType)
        => (entityType is RuntimeEntityType)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)entityType[RelationalAnnotationNames.Comment];

    /// <summary>
    ///     Configures a comment to be applied to the table this entity is mapped to.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="comment">The comment for the table this entity is mapped to.</param>
    public static void SetComment(this IMutableEntityType entityType, string? comment)
        => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment);

    /// <summary>
    ///     Configures a comment to be applied to the table this entity is mapped to.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="comment">The comment for the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured comment.</returns>
    public static string? SetComment(
        this IConventionEntityType entityType,
        string? comment,
        bool fromDataAnnotation = false)
    {
        entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment, fromDataAnnotation);

        return comment;
    }

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the table comment.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the table comment.</returns>
    public static ConfigurationSource? GetCommentConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.Comment)
            ?.GetConfigurationSource();

    #endregion Comment

    /// <summary>
    ///     <para>
    ///         Returns all configured entity type mapping fragments.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configured entity type mapping fragments.</returns>
    public static IEnumerable<IReadOnlyEntityTypeMappingFragment> GetMappingFragments(this IReadOnlyEntityType entityType)
        => EntityTypeMappingFragment.Get(entityType) ?? Enumerable.Empty<IReadOnlyEntityTypeMappingFragment>();

    /// <summary>
    ///     <para>
    ///         Returns all configured entity type mapping fragments.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configured entity type mapping fragments.</returns>
    public static IEnumerable<IEntityTypeMappingFragment> GetMappingFragments(this IEntityType entityType)
        => EntityTypeMappingFragment.Get(entityType)?.Cast<IEntityTypeMappingFragment>()
         ?? Enumerable.Empty<IEntityTypeMappingFragment>();

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IReadOnlyEntityTypeMappingFragment? FindMappingFragment(
        this IReadOnlyEntityType entityType, in StoreObjectIdentifier storeObject)
        => EntityTypeMappingFragment.Find(entityType, storeObject);

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IMutableEntityTypeMappingFragment? FindMappingFragment(
        this IMutableEntityType entityType, in StoreObjectIdentifier storeObject)
        => (IMutableEntityTypeMappingFragment?)EntityTypeMappingFragment.Find(entityType, storeObject);

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IConventionEntityTypeMappingFragment? FindMappingFragment(
        this IConventionEntityType entityType, in StoreObjectIdentifier storeObject)
        => (IConventionEntityTypeMappingFragment?)EntityTypeMappingFragment.Find(entityType, storeObject);

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IEntityTypeMappingFragment? FindMappingFragment(
        this IEntityType entityType, in StoreObjectIdentifier storeObject)
        => (IEntityTypeMappingFragment?)EntityTypeMappingFragment.Find(entityType, storeObject);

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IMutableEntityTypeMappingFragment GetOrCreateMappingFragment(
        this IMutableEntityType entityType,
        in StoreObjectIdentifier storeObject)
        => EntityTypeMappingFragment.GetOrCreate(entityType, storeObject, ConfigurationSource.Explicit);

    /// <summary>
    ///     <para>
    ///         Returns the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>An object that represents an entity type mapping fragment.</returns>
    public static IConventionEntityTypeMappingFragment GetOrCreateMappingFragment(
        this IConventionEntityType entityType,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => EntityTypeMappingFragment.GetOrCreate((IMutableEntityType)entityType, storeObject,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     <para>
    ///         Removes the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>
    ///     The removed <see cref="IMutableEntityTypeMappingFragment" /> or <see langword="null" />
    ///     if no overrides for the given store object were found.
    /// </returns>
    public static IMutableEntityTypeMappingFragment? RemoveMappingFragment(
        this IMutableEntityType entityType,
        in StoreObjectIdentifier storeObject)
        => EntityTypeMappingFragment.Remove(entityType, storeObject, ConfigurationSource.Explicit);

    /// <summary>
    ///     <para>
    ///         Removes the entity type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The removed <see cref="IConventionEntityTypeMappingFragment" /> or <see langword="null" />
    ///     if no overrides for the given store object were found or the existing overrides were configured from a higher source.
    /// </returns>
    public static IConventionEntityTypeMappingFragment? RemoveMappingFragment(
        this IConventionEntityType entityType,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => EntityTypeMappingFragment.Remove((IMutableEntityType)entityType, storeObject,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Gets the foreign keys for the given entity type that point to other entity types
    ///     sharing the same table-like store object.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    public static IEnumerable<IReadOnlyForeignKey> FindRowInternalForeignKeys(
        this IReadOnlyEntityType entityType,
        StoreObjectIdentifier storeObject)
    {
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
        {
            yield break;
        }

        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            var principalEntityType = foreignKey.PrincipalEntityType;
            if (!foreignKey.PrincipalKey.IsPrimaryKey()
                || principalEntityType == foreignKey.DeclaringEntityType
                || !foreignKey.IsUnique
#pragma warning disable EF1001 // Internal EF Core API usage.
                || !PropertyListComparer.Instance.Equals(foreignKey.Properties, primaryKey.Properties))
#pragma warning restore EF1001 // Internal EF Core API usage.
            {
                continue;
            }

            switch (storeObject.StoreObjectType)
            {
                case StoreObjectType.Table:
                    if (storeObject.Name == principalEntityType.GetTableName()
                        && storeObject.Schema == principalEntityType.GetSchema())
                    {
                        yield return foreignKey;
                    }

                    break;
                case StoreObjectType.View:
                    if (storeObject.Name == principalEntityType.GetViewName()
                        && storeObject.Schema == principalEntityType.GetViewSchema())
                    {
                        yield return foreignKey;
                    }

                    break;
                case StoreObjectType.Function:
                    if (storeObject.Name == principalEntityType.GetFunctionName())
                    {
                        yield return foreignKey;
                    }

                    break;
                default:
                    throw new NotSupportedException(storeObject.StoreObjectType.ToString());
            }
        }
    }

    /// <summary>
    ///     Gets the foreign keys for the given entity type that point to other entity types
    ///     sharing the same table-like store object.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    public static IEnumerable<IMutableForeignKey> FindRowInternalForeignKeys(
            this IMutableEntityType entityType,
            in StoreObjectIdentifier storeObject)
        // ReSharper disable once RedundantCast
        => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IMutableForeignKey>();

    /// <summary>
    ///     Gets the foreign keys for the given entity type that point to other entity types
    ///     sharing the same table-like store object.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    public static IEnumerable<IConventionForeignKey> FindRowInternalForeignKeys(
            this IConventionEntityType entityType,
            in StoreObjectIdentifier storeObject)
        // ReSharper disable once RedundantCast
        => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IConventionForeignKey>();

    /// <summary>
    ///     Gets the foreign keys for the given entity type that point to other entity types
    ///     sharing the same table-like store object.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    public static IEnumerable<IForeignKey> FindRowInternalForeignKeys(
            this IEntityType entityType,
            in StoreObjectIdentifier storeObject)
        // ReSharper disable once RedundantCast
        => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IForeignKey>();

    #region IsTableExcludedFromMigrations

    /// <summary>
    ///     Gets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>A value indicating whether the associated table is ignored by Migrations.</returns>
    public static bool IsTableExcludedFromMigrations(this IReadOnlyEntityType entityType)
    {
        if (entityType is RuntimeEntityType)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var excluded = (bool?)entityType[RelationalAnnotationNames.IsTableExcludedFromMigrations];
        if (excluded != null)
        {
            return excluded.Value;
        }

        if (entityType.BaseType != null)
        {
            return entityType.GetRootType().IsTableExcludedFromMigrations();
        }

        var ownership = entityType.FindOwnership();
        if (ownership != null
            && ownership.IsUnique)
        {
            return ownership.PrincipalEntityType.IsTableExcludedFromMigrations();
        }

        return false;
    }

    /// <summary>
    ///     Gets a value indicating whether the specified table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <returns>A value indicating whether the associated table is ignored by Migrations.</returns>
    public static bool IsTableExcludedFromMigrations(
        this IReadOnlyEntityType entityType,
        in StoreObjectIdentifier storeObject)
    {
        if (entityType is RuntimeEntityType)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var overrides = entityType.FindMappingFragment(storeObject);
        if (overrides != null)
        {
            return overrides.IsTableExcludedFromMigrations ?? entityType.IsTableExcludedFromMigrations();
        }

        if (StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject)
        {
            return entityType.IsTableExcludedFromMigrations();
        }

        throw new InvalidOperationException(
            RelationalStrings.TableNotMappedEntityType(entityType.DisplayName(), storeObject.DisplayName()));
    }

    /// <summary>
    ///     Sets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="excluded">A value indicating whether the associated table is ignored by Migrations.</param>
    public static void SetIsTableExcludedFromMigrations(this IMutableEntityType entityType, bool? excluded)
        => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.IsTableExcludedFromMigrations, excluded);

    /// <summary>
    ///     Sets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="excluded">A value indicating whether the associated table is ignored by Migrations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsTableExcludedFromMigrations(
        this IConventionEntityType entityType,
        bool? excluded,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.IsTableExcludedFromMigrations, excluded, fromDataAnnotation)
            ?.Value;

    /// <summary>
    ///     Sets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="excluded">A value indicating whether the associated table is ignored by Migrations.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    public static void SetIsTableExcludedFromMigrations(
        this IMutableEntityType entityType,
        bool? excluded,
        in StoreObjectIdentifier storeObject)
        => entityType.GetOrCreateMappingFragment(storeObject).IsTableExcludedFromMigrations = excluded;

    /// <summary>
    ///     Sets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="excluded">A value indicating whether the associated table is ignored by Migrations.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsTableExcludedFromMigrations(
        this IConventionEntityType entityType,
        bool? excluded,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => entityType.GetOrCreateMappingFragment(storeObject, fromDataAnnotation).SetIsTableExcludedFromMigrations(
                excluded, fromDataAnnotation);
        
    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations(IReadOnlyEntityType)" />.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>
    ///     The <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations(IReadOnlyEntityType)" />.
    /// </returns>
    public static ConfigurationSource? GetIsTableExcludedFromMigrationsConfigurationSource(
        this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.IsTableExcludedFromMigrations)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations(IReadOnlyEntityType, in StoreObjectIdentifier)" />.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <returns>
    ///     The <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations(IReadOnlyEntityType, in StoreObjectIdentifier)" />.
    /// </returns>
    public static ConfigurationSource? GetIsTableExcludedFromMigrationsConfigurationSource(
        this IConventionEntityType entityType,
        in StoreObjectIdentifier storeObject)
        => entityType.FindMappingFragment(storeObject)?.GetIsTableExcludedFromMigrationsConfigurationSource();

    /// <summary>
    ///     Gets the mapping strategy for the derived types.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The mapping strategy for the derived types.</returns>
    public static string? GetMappingStrategy(this IReadOnlyEntityType entityType)
        => (string?)entityType[RelationalAnnotationNames.MappingStrategy]
            ?? (entityType.BaseType != null
                ? entityType.GetRootType().GetMappingStrategy()
                : entityType.GetDiscriminatorPropertyName() != null
                    ? RelationalAnnotationNames.TphMappingStrategy
                    : entityType.FindPrimaryKey() == null || !entityType.GetDirectlyDerivedTypes().Any()
                        ? null
                        : RelationalAnnotationNames.TptMappingStrategy);

    #endregion IsTableExcludedFromMigrations

    #region Mapping strategy

    /// <summary>
    ///     Sets the mapping strategy for the derived types.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="strategy">The mapping strategy for the derived types.</param>
    public static void SetMappingStrategy(this IMutableEntityType entityType, string? strategy)
        => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.MappingStrategy, strategy);

    /// <summary>
    ///     Sets the mapping strategy for the derived types.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="strategy">The mapping strategy for the derived types.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetMappingStrategy(
        this IConventionEntityType entityType,
        string? strategy,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.MappingStrategy, strategy, fromDataAnnotation)
            ?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="GetMappingStrategy" />.
    /// </summary>
    /// <param name="entityType">The entity type to find configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for <see cref="GetMappingStrategy" />.</returns>
    public static ConfigurationSource? GetMappingStrategyConfigurationSource(
        this IConventionEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.MappingStrategy)
            ?.GetConfigurationSource();

    #endregion Mapping strategy

    #region Trigger

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the sequence on.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    public static IReadOnlyTrigger? FindTrigger(this IReadOnlyEntityType entityType, string name)
        => Trigger.FindTrigger(entityType, Check.NotEmpty(name, nameof(name)));

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the sequence on.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    public static IMutableTrigger? FindTrigger(this IMutableEntityType entityType, string name)
        => (IMutableTrigger?)((IReadOnlyEntityType)entityType).FindTrigger(name);

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the sequence on.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    public static IConventionTrigger? FindTrigger(this IConventionEntityType entityType, string name)
        => (IConventionTrigger?)((IReadOnlyEntityType)entityType).FindTrigger(name);

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="entityType">The entity type to find the sequence on.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    public static ITrigger? FindTrigger(this IEntityType entityType, string name)
        => (ITrigger?)((IReadOnlyEntityType)entityType).FindTrigger(name);

    /// <summary>
    ///     Creates a new trigger with the given name on entity type. Throws an exception if a trigger with the same name exists on the same
    ///     entity type.
    /// </summary>
    /// <param name="entityType">The entity type to add the trigger to.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>The trigger.</returns>
    public static IMutableTrigger AddTrigger(this IMutableEntityType entityType, string name)
    {
        Check.NotEmpty(name, nameof(name));

        return new Trigger(entityType, name, tableName: null, tableSchema: null, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     Creates a new trigger with the given name on entity type. Throws an exception if a trigger with the same name exists on the same
    ///     entity type.
    /// </summary>
    /// <param name="entityType">The entity type to add the trigger to.</param>
    /// <param name="name">The trigger name.</param>
    /// <param name="tableName">The name of the table on which this trigger is defined.</param>
    /// <param name="tableSchema">The schema of the table on which this trigger is defined.</param>
    /// <returns>The trigger.</returns>
    public static IMutableTrigger AddTrigger(this IMutableEntityType entityType, string name, string tableName, string? tableSchema = null)
    {
        Check.NotEmpty(name, nameof(name));

        return new Trigger(entityType, name, tableName, tableSchema, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     Creates a new trigger with the given name on entity type. Throws an exception if a trigger with the same name exists on the same
    ///     entity type.
    /// </summary>
    /// <param name="entityType">The entityType to add the trigger to.</param>
    /// <param name="name">The trigger name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The trigger.</returns>
    public static IConventionTrigger AddTrigger(
        this IConventionEntityType entityType,
        string name,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(name, nameof(name));

        return new Trigger(
            (IMutableEntityType)entityType, name, tableName: null, tableSchema: null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }

    /// <summary>
    ///     Creates a new trigger with the given name on entity type. Throws an exception if a trigger with the same name exists on the same
    ///     entity type.
    /// </summary>
    /// <param name="entityType">The entityType to add the trigger to.</param>
    /// <param name="name">The trigger name.</param>
    /// <param name="tableName">The name of the table on which this trigger is defined.</param>
    /// <param name="tableSchema">The schema of the table on which this trigger is defined.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The trigger.</returns>
    public static IConventionTrigger AddTrigger(
        this IConventionEntityType entityType,
        string name,
        string tableName,
        string? tableSchema,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(name, nameof(name));

        return new Trigger(
            (IMutableEntityType)entityType, name, tableName, tableSchema,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }

    /// <summary>
    ///     Removes the <see cref="IMutableTrigger" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entityType to find the trigger in.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>
    ///     The removed <see cref="IMutableTrigger" /> or <see langword="null" /> if no trigger with the given name was found.
    /// </returns>
    public static IMutableTrigger? RemoveTrigger(this IMutableEntityType entityType, string name)
        => Trigger.RemoveTrigger(entityType, name);

    /// <summary>
    ///     Removes the <see cref="IConventionTrigger" /> with the given name.
    /// </summary>
    /// <param name="entityType">The entityType to find the trigger in.</param>
    /// <param name="name">The trigger name.</param>
    /// <returns>
    ///     The removed <see cref="IMutableTrigger" /> or <see langword="null" /> if no trigger with the given name was found
    ///     or the existing trigger was configured from a higher source.
    /// </returns>
    public static IConventionTrigger? RemoveTrigger(this IConventionEntityType entityType, string name)
        => Trigger.RemoveTrigger((IMutableEntityType)entityType, name);

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    public static IEnumerable<IReadOnlyTrigger> GetTriggers(this IReadOnlyEntityType entityType)
        => Trigger.GetTriggers(entityType);

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    public static IEnumerable<IMutableTrigger> GetTriggers(this IMutableEntityType entityType)
        => Trigger.GetTriggers(entityType).Cast<IMutableTrigger>();

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    public static IEnumerable<IConventionTrigger> GetTriggers(this IConventionEntityType entityType)
        => Trigger.GetTriggers(entityType).Cast<IConventionTrigger>();

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    public static IEnumerable<ITrigger> GetTriggers(this IEntityType entityType)
        => Trigger.GetTriggers(entityType).Cast<ITrigger>();

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    /// <remarks>
    ///     This method does not return triggers declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same trigger more than once.
    ///     Use <see cref="GetTriggers(IReadOnlyEntityType)" /> to also return triggers declared on base types.
    /// </remarks>
    public static IEnumerable<IReadOnlyTrigger> GetDeclaredTriggers(this IReadOnlyEntityType entityType)
        => Trigger.GetDeclaredTriggers(entityType);

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    /// <remarks>
    ///     This method does not return triggers declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same trigger more than once.
    ///     Use <see cref="GetTriggers(IMutableEntityType)" /> to also return triggers declared on base types.
    /// </remarks>
    public static IEnumerable<IMutableTrigger> GetDeclaredTriggers(this IMutableEntityType entityType)
        => Trigger.GetDeclaredTriggers(entityType).Cast<IMutableTrigger>();

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    /// <remarks>
    ///     This method does not return triggers declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same trigger more than once.
    ///     Use <see cref="GetTriggers(IConventionEntityType)" /> to also return triggers declared on base types.
    /// </remarks>
    public static IEnumerable<IConventionTrigger> GetDeclaredTriggers(this IConventionEntityType entityType)
        => Trigger.GetDeclaredTriggers(entityType).Cast<IConventionTrigger>();

    /// <summary>
    ///     Returns all triggers on the entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the triggers on.</param>
    /// <remarks>
    ///     This method does not return triggers declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same trigger more than once.
    ///     Use <see cref="GetTriggers(IEntityType)" /> to also return triggers declared on base types.
    /// </remarks>
    public static IEnumerable<ITrigger> GetDeclaredTriggers(this IEntityType entityType)
        => Trigger.GetDeclaredTriggers(entityType).Cast<ITrigger>();

    #endregion Trigger
}
