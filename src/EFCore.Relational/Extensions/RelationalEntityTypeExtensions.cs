// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Entity type extension methods for relational database metadata.
    /// </summary>
    public static class RelationalEntityTypeExtensions
    {
        /// <summary>
        ///     Gets the name used for the <see cref="ISqlQuery" /> mapped using
        ///     <see cref="M:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
        /// </summary>
        public static readonly string DefaultQueryNameBase = "MappedSqlQuery";

        /// <summary>
        ///     Returns the name of the table to which the entity type is mapped
        ///     or <see langword="null" /> if not mapped to a table.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The name of the table to which the entity type is mapped. </returns>
        public static string? GetTableName(this IReadOnlyEntityType entityType)
        {
            var nameAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.TableName);
            if (nameAnnotation != null)
            {
                return (string?)nameAnnotation.Value;
            }

            if (entityType.BaseType != null)
            {
                return entityType.GetRootType().GetTableName();
            }

            return (entityType as IConventionEntityType)?.GetViewNameConfigurationSource() == null
                && ((entityType as IConventionEntityType)?.GetFunctionNameConfigurationSource() == null)
#pragma warning disable CS0618 // Type or member is obsolete
                && ((entityType as IConventionEntityType)?.GetDefiningQueryConfigurationSource() == null)
#pragma warning restore CS0618 // Type or member is obsolete
                && ((entityType as IConventionEntityType)?.GetSqlQueryConfigurationSource() == null)
                    ? GetDefaultTableName(entityType)
                    : null;
        }

        /// <summary>
        ///     Returns the default table name that would be used for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <param name="truncate"> A value indicating whether the name should be truncated to the max identifier length. </param>
        /// <returns> The default name of the table to which the entity type would be mapped. </returns>
        public static string? GetDefaultTableName(this IReadOnlyEntityType entityType, bool truncate = true)
        {
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

            return truncate
                ? Uniquifier.Truncate(name, entityType.Model.GetMaxIdentifierLength())
                : name;
        }

        /// <summary>
        ///     Sets the name of the table to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the table name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetTableName(this IMutableEntityType entityType, string? name)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.TableName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name of the table to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the table name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured table name. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the table name. </returns>
        public static ConfigurationSource? GetTableNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.TableName)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the database schema that contains the mapped table.
        /// </summary>
        /// <param name="entityType"> The entity type to get the schema for. </param>
        /// <returns> The database schema that contains the mapped table. </returns>
        public static string? GetSchema(this IReadOnlyEntityType entityType)
        {
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
        /// <param name="entityType"> The entity type to get the schema for. </param>
        /// <returns> The default database schema to which the entity type would be mapped. </returns>
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
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetSchema(this IMutableEntityType entityType, string? value)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.Schema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database schema that contains the mapped table.
        /// </summary>
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the database schema. </returns>
        public static ConfigurationSource? GetSchemaConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.Schema)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the name of the table to which the entity type is mapped prepended by the schema
        ///     or <see langword="null" /> if not mapped to a table.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The name of the table to which the entity type is mapped prepended by the schema. </returns>
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
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The name of the table to which the entity type is mapped prepended by the schema. </returns>
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
        /// <param name="entityType"> The entity type to get the table mappings for. </param>
        /// <returns> The tables to which the entity type is mapped. </returns>
        public static IEnumerable<ITableMappingBase> GetDefaultMappings(this IEntityType entityType)
            => (IEnumerable<ITableMappingBase>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DefaultMappings)
                ?? Array.Empty<ITableMappingBase>();

        /// <summary>
        ///     Returns the tables to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table mappings for. </param>
        /// <returns> The tables to which the entity type is mapped. </returns>
        public static IEnumerable<ITableMapping> GetTableMappings(this IEntityType entityType)
            => (IEnumerable<ITableMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableMappings)
                ?? Array.Empty<ITableMapping>();

        /// <summary>
        ///     Returns the name of the view to which the entity type is mapped or <see langword="null" /> if not mapped to a view.
        /// </summary>
        /// <param name="entityType"> The entity type to get the view name for. </param>
        /// <returns> The name of the view to which the entity type is mapped. </returns>
        public static string? GetViewName(this IReadOnlyEntityType entityType)
        {
            var nameAnnotation = (string?)entityType[RelationalAnnotationNames.ViewName];
            if (nameAnnotation != null)
            {
                return nameAnnotation;
            }

            if (entityType.BaseType != null)
            {
                return entityType.GetRootType().GetViewName();
            }

            return ((entityType as IConventionEntityType)?.GetFunctionNameConfigurationSource() == null)
#pragma warning disable CS0618 // Type or member is obsolete
                && (entityType as IConventionEntityType)?.GetDefiningQueryConfigurationSource() == null
#pragma warning restore CS0618 // Type or member is obsolete
                && ((entityType as IConventionEntityType)?.GetSqlQueryConfigurationSource() == null)
                    ? GetDefaultViewName(entityType)
                    : null;
        }

        /// <summary>
        ///     Returns the default view name that would be used for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The default name of the table to which the entity type would be mapped. </returns>
        public static string? GetDefaultViewName(this IReadOnlyEntityType entityType)
        {
            var ownership = entityType.FindOwnership();
            return ownership != null
                && ownership.IsUnique
                    ? ownership.PrincipalEntityType.GetViewName()
                    : null;
        }

        /// <summary>
        ///     Sets the name of the view to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the view name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetViewName(this IMutableEntityType entityType, string? name)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.ViewName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name of the view to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the view name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the view name. </returns>
        public static ConfigurationSource? GetViewNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.ViewName)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the database schema that contains the mapped view.
        /// </summary>
        /// <param name="entityType"> The entity type to get the view schema for. </param>
        /// <returns> The database schema that contains the mapped view. </returns>
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
        /// <param name="entityType"> The entity type to get the view schema for. </param>
        /// <returns> The default database schema to which the entity type would be mapped. </returns>
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
        /// <param name="entityType"> The entity type to set the view schema for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetViewSchema(this IMutableEntityType entityType, string? value)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.ViewSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database schema that contains the mapped view.
        /// </summary>
        /// <param name="entityType"> The entity type to set the view schema for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured schema. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the view schema. </returns>
        public static ConfigurationSource? GetViewSchemaConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.ViewSchema)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the views to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the view mappings for. </param>
        /// <returns> The views to which the entity type is mapped. </returns>
        public static IEnumerable<IViewMapping> GetViewMappings(this IEntityType entityType)
            => (IEnumerable<IViewMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewMappings)
                ?? Array.Empty<IViewMapping>();

        /// <summary>
        ///     Gets the default SQL query name that would be used for this entity type when mapped using
        ///     <see cref="M:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Gets the default SQL query name. </returns>
        public static string GetDefaultSqlQueryName(this IReadOnlyEntityType entityType)
            => entityType.Name + "." + DefaultQueryNameBase;

        /// <summary>
        ///     Returns the SQL string used to provide data for the entity type or <see langword="null" /> if not mapped to a SQL string.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The SQL string used to provide data for the entity type. </returns>
        public static string? GetSqlQuery(this IReadOnlyEntityType entityType)
        {
            var queryAnnotation = (string?)entityType[RelationalAnnotationNames.SqlQuery];
            if (queryAnnotation != null)
            {
                return queryAnnotation;
            }

            if (entityType.BaseType != null)
            {
                return entityType.GetRootType().GetSqlQuery();
            }

            return null;
        }

        /// <summary>
        ///     Sets the SQL string used to provide data for the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The SQL string to set. </param>
        public static void SetSqlQuery(this IMutableEntityType entityType, string? name)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.SqlQuery,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the SQL string used to provide data for the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="name"> The SQL string to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the query SQL string. </returns>
        public static ConfigurationSource? GetSqlQueryConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.SqlQuery)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the SQL string mappings.
        /// </summary>
        /// <param name="entityType"> The entity type to get the function mappings for. </param>
        /// <returns> The functions to which the entity type is mapped. </returns>
        public static IEnumerable<ISqlQueryMapping> GetSqlQueryMappings(this IEntityType entityType)
            => (IEnumerable<ISqlQueryMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.SqlQueryMappings)
                ?? Array.Empty<ISqlQueryMapping>();

        /// <summary>
        ///     Returns the name of the function to which the entity type is mapped or <see langword="null" /> if not mapped to a function.
        /// </summary>
        /// <param name="entityType"> The entity type to get the function name for. </param>
        /// <returns> The name of the function to which the entity type is mapped. </returns>
        public static string? GetFunctionName(this IReadOnlyEntityType entityType)
        {
            var nameAnnotation = (string?)entityType[RelationalAnnotationNames.FunctionName];
            if (nameAnnotation != null)
            {
                return nameAnnotation;
            }

            if (entityType.BaseType != null)
            {
                return entityType.GetRootType().GetFunctionName();
            }

            return null;
        }

        /// <summary>
        ///     Sets the name of the function to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the function name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetFunctionName(this IMutableEntityType entityType, string? name)
            => entityType.SetAnnotation(
                RelationalAnnotationNames.FunctionName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name of the function to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the function name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the function name. </returns>
        public static ConfigurationSource? GetFunctionNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.FunctionName)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the functions to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the function mappings for. </param>
        /// <returns> The functions to which the entity type is mapped. </returns>
        public static IEnumerable<IFunctionMapping> GetFunctionMappings(this IEntityType entityType)
            => (IEnumerable<IFunctionMapping>?)entityType.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.FunctionMappings)
                ?? Array.Empty<IFunctionMapping>();

        /// <summary>
        ///     Finds an <see cref="IReadOnlyCheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
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
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
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
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
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
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
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
        /// <param name="entityType"> The entity type to add the check constraint to. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> The new check constraint. </returns>
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
        /// <param name="entityType"> The entity type to add the check constraint to. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new check constraint. </returns>
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
        /// <param name="entityType"> The entity type to remove the check constraint from. </param>
        /// <param name="name"> The check constraint name to be removed. </param>
        /// <returns> The removed check constraint. </returns>
        public static IMutableCheckConstraint? RemoveCheckConstraint(
            this IMutableEntityType entityType,
            string name)
            => CheckConstraint.RemoveCheckConstraint(entityType, Check.NotEmpty(name, nameof(name)));

        /// <summary>
        ///     Removes the <see cref="IConventionCheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the check constraint from. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <returns> The removed check constraint. </returns>
        public static IConventionCheckConstraint? RemoveCheckConstraint(
            this IConventionEntityType entityType,
            string name)
            => (IConventionCheckConstraint?)CheckConstraint.RemoveCheckConstraint(
                (IMutableEntityType)entityType, Check.NotEmpty(name, nameof(name)));

        /// <summary>
        ///     Returns all check constraints contained in the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IReadOnlyCheckConstraint> GetCheckConstraints(this IReadOnlyEntityType entityType)
            => CheckConstraint.GetCheckConstraints(entityType);

        /// <summary>
        ///     Returns all check constraints contained in the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IMutableCheckConstraint> GetCheckConstraints(this IMutableEntityType entityType)
            => CheckConstraint.GetCheckConstraints(entityType).Cast<IMutableCheckConstraint>();

        /// <summary>
        ///     Returns all check constraints contained in the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IConventionCheckConstraint> GetCheckConstraints(this IConventionEntityType entityType)
            => CheckConstraint.GetCheckConstraints(entityType).Cast<IConventionCheckConstraint>();

        /// <summary>
        ///     Returns all check constraints contained in the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<ICheckConstraint> GetCheckConstraints(this IEntityType entityType)
            => CheckConstraint.GetCheckConstraints(entityType).Cast<ICheckConstraint>();

        /// <summary>
        ///     <para>
        ///         Returns all check constraints declared on the entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return check constraints declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
        ///         Use <see cref="GetCheckConstraints(IReadOnlyEntityType)" /> to also return check constraints declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IReadOnlyCheckConstraint> GetDeclaredCheckConstraints(this IReadOnlyEntityType entityType)
            => CheckConstraint.GetDeclaredCheckConstraints(entityType);

        /// <summary>
        ///     <para>
        ///         Returns all check constraints declared on the entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return check constraints declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
        ///         Use <see cref="GetCheckConstraints(IMutableEntityType)" /> to also return check constraints declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IMutableCheckConstraint> GetDeclaredCheckConstraints(this IMutableEntityType entityType)
            => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<IMutableCheckConstraint>();

        /// <summary>
        ///     <para>
        ///         Returns all check constraints declared on the entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return check constraints declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
        ///         Use <see cref="GetCheckConstraints(IConventionEntityType)" /> to also return check constraints declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<IConventionCheckConstraint> GetDeclaredCheckConstraints(this IConventionEntityType entityType)
            => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<IConventionCheckConstraint>();

        /// <summary>
        ///     <para>
        ///         Returns all check constraints declared on the entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return check constraints declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same check constraint more than once.
        ///         Use <see cref="GetCheckConstraints(IEntityType)" /> to also return check constraints declared on base types.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<ICheckConstraint> GetDeclaredCheckConstraints(this IEntityType entityType)
            => CheckConstraint.GetDeclaredCheckConstraints(entityType).Cast<ICheckConstraint>();

        /// <summary>
        ///     Returns the comment for the table this entity is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The comment for the table this entity is mapped to. </returns>
        public static string? GetComment(this IReadOnlyEntityType entityType)
            => entityType is RuntimeEntityType
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)entityType[RelationalAnnotationNames.Comment];

        /// <summary>
        ///     Configures a comment to be applied to the table this entity is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="comment"> The comment for the table this entity is mapped to. </param>
        public static void SetComment(this IMutableEntityType entityType, string? comment)
            => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment);

        /// <summary>
        ///     Configures a comment to be applied to the table this entity is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured comment. </returns>
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
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the table comment. </returns>
        public static ConfigurationSource? GetCommentConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.Comment)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Gets the foreign keys for the given entity type that point to other entity types
        ///     sharing the same table-like store object.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
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
        /// <param name="entityType"> The entity type. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        public static IEnumerable<IMutableForeignKey> FindRowInternalForeignKeys(
                this IMutableEntityType entityType,
                in StoreObjectIdentifier storeObject)
            // ReSharper disable once RedundantCast
            => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IMutableForeignKey>();

        /// <summary>
        ///     Gets the foreign keys for the given entity type that point to other entity types
        ///     sharing the same table-like store object.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        public static IEnumerable<IConventionForeignKey> FindRowInternalForeignKeys(
                this IConventionEntityType entityType,
                in StoreObjectIdentifier storeObject)
            // ReSharper disable once RedundantCast
            => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IConventionForeignKey>();

        /// <summary>
        ///     Gets the foreign keys for the given entity type that point to other entity types
        ///     sharing the same table-like store object.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        public static IEnumerable<IForeignKey> FindRowInternalForeignKeys(
                this IEntityType entityType,
                in StoreObjectIdentifier storeObject)
            // ReSharper disable once RedundantCast
            => ((IReadOnlyEntityType)entityType).FindRowInternalForeignKeys(storeObject).Cast<IForeignKey>();

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
        ///     Sets a value indicating whether the associated table is ignored by Migrations.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="excluded"> A value indicating whether the associated table is ignored by Migrations. </param>
        public static void SetIsTableExcludedFromMigrations(this IMutableEntityType entityType, bool? excluded)
            => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.IsTableExcludedFromMigrations, excluded);

        /// <summary>
        ///     Sets a value indicating whether the associated table is ignored by Migrations.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="excluded"> A value indicating whether the associated table is ignored by Migrations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsTableExcludedFromMigrations(
            this IConventionEntityType entityType,
            bool? excluded,
            bool fromDataAnnotation = false)
            => (bool?)entityType.SetOrRemoveAnnotation(
                    RelationalAnnotationNames.IsTableExcludedFromMigrations, excluded, fromDataAnnotation)
                ?.Value;

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for <see cref="IsTableExcludedFromMigrations" />. </returns>
        public static ConfigurationSource? GetIsTableExcludedFromMigrationsConfigurationSource(
            this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.IsTableExcludedFromMigrations)
                ?.GetConfigurationSource();
    }
}
