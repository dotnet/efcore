// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.EntityFrameworkCore;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
///     Type extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalTypeBaseExtensions
{
    #region Table mapping

    /// <summary>
    ///     Returns the name of the table to which the type is mapped
    ///     or <see langword="null" /> if not mapped to a table.
    /// </summary>
    /// <param name="typeBase">The type to get the table name for.</param>
    /// <returns>The name of the table to which the type is mapped.</returns>
    public static string? GetTableName(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetTableName();

    /// <summary>
    ///     Returns the database schema that contains the mapped table.
    /// </summary>
    /// <param name="typeBase">The type to get the schema for.</param>
    /// <returns>The database schema that contains the mapped table.</returns>
    public static string? GetSchema(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetSchema();

    /// <summary>
    ///     Returns the default mappings that the type would use.
    /// </summary>
    /// <param name="typeBase">The type to get the table mappings for.</param>
    /// <returns>The tables to which the type is mapped.</returns>
    public static IEnumerable<ITableMappingBase> GetDefaultMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<ITableMappingBase>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DefaultMappings)
            ?? [];
    }

    /// <summary>
    ///     Returns the tables to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to get the table mappings for.</param>
    /// <returns>The tables to which the type is mapped.</returns>
    public static IEnumerable<ITableMapping> GetTableMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<ITableMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableMappings)
            ?? [];
    }

    #endregion Table mapping

    #region View mapping

    /// <summary>
    ///     Returns the name of the view to which the type is mapped or <see langword="null" /> if not mapped to a view.
    /// </summary>
    /// <param name="typeBase">The type to get the view name for.</param>
    /// <returns>The name of the view to which the type is mapped.</returns>
    public static string? GetViewName(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetViewName();

    /// <summary>
    ///     Returns the database schema that contains the mapped view.
    /// </summary>
    /// <param name="typeBase">The type to get the view schema for.</param>
    /// <returns>The database schema that contains the mapped view.</returns>
    public static string? GetViewSchema(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetViewSchema();

    /// <summary>
    ///     Returns the views to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to get the view mappings for.</param>
    /// <returns>The views to which the type is mapped.</returns>
    public static IEnumerable<IViewMapping> GetViewMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<IViewMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewMappings)
            ?? [];
    }

    #endregion View mapping

    #region SQL query mapping

    /// <summary>
    ///     Returns the SQL string used to provide data for the type or <see langword="null" /> if not mapped to a SQL string.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The SQL string used to provide data for the type.</returns>
    public static string? GetSqlQuery(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetSqlQuery();

    /// <summary>
    ///     Returns the SQL string mappings.
    /// </summary>
    /// <param name="typeBase">The type to get the SQL string mappings for.</param>
    /// <returns>The SQL string to which the type is mapped.</returns>
    public static IEnumerable<ISqlQueryMapping> GetSqlQueryMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<ISqlQueryMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.SqlQueryMappings)
            ?? [];
    }

    #endregion SQL query mapping

    #region Function mapping

    /// <summary>
    ///     Returns the name of the function to which the type is mapped or <see langword="null" /> if not mapped to a function.
    /// </summary>
    /// <param name="typeBase">The type to get the function name for.</param>
    /// <returns>The name of the function to which the type is mapped.</returns>
    public static string? GetFunctionName(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetFunctionName();

    /// <summary>
    ///     Returns the functions to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to get the function mappings for.</param>
    /// <returns>The functions to which the type is mapped.</returns>
    public static IEnumerable<IFunctionMapping> GetFunctionMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<IFunctionMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.FunctionMappings)
            ?? [];
    }

    #endregion

    #region SProc mapping

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for deletes
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IReadOnlyStoredProcedure? GetDeleteStoredProcedure(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetDeleteStoredProcedure();

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for deletes
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IStoredProcedure? GetDeleteStoredProcedure(this ITypeBase typeBase)
        => typeBase.ContainingEntityType.GetDeleteStoredProcedure();

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for inserts
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IReadOnlyStoredProcedure? GetInsertStoredProcedure(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetInsertStoredProcedure();

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for inserts
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IStoredProcedure? GetInsertStoredProcedure(this ITypeBase typeBase)
        => typeBase.ContainingEntityType.GetInsertStoredProcedure();

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for updates
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IReadOnlyStoredProcedure? GetUpdateStoredProcedure(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetUpdateStoredProcedure();

    /// <summary>
    ///     Returns the stored procedure to which the type is mapped for updates
    ///     or <see langword="null" /> if not mapped to a stored procedure.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The stored procedure to which the type is mapped.</returns>
    public static IStoredProcedure? GetUpdateStoredProcedure(this ITypeBase typeBase)
        => typeBase.ContainingEntityType.GetUpdateStoredProcedure();

    /// <summary>
    ///     Returns the insert stored procedures to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The insert stored procedures to which the type is mapped.</returns>
    public static IEnumerable<IStoredProcedureMapping> GetInsertStoredProcedureMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.InsertStoredProcedureMappings)
            ?? [];
    }

    /// <summary>
    ///     Returns the delete stored procedures to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The delete stored procedures to which the type is mapped.</returns>
    public static IEnumerable<IStoredProcedureMapping> GetDeleteStoredProcedureMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DeleteStoredProcedureMappings)
            ?? [];
    }

    /// <summary>
    ///     Returns the update stored procedures to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The update stored procedures to which the type is mapped.</returns>
    public static IEnumerable<IStoredProcedureMapping> GetUpdateStoredProcedureMappings(this ITypeBase typeBase)
    {
        typeBase.Model.EnsureRelationalModel();
        return (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.UpdateStoredProcedureMappings)
            ?? [];
    }

    #endregion

    #region Mapping Fragments

    /// <summary>
    ///     <para>
    ///         Returns all configured type mapping fragments.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The configured type mapping fragments.</returns>
    public static IEnumerable<IReadOnlyEntityTypeMappingFragment> GetMappingFragments(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetMappingFragments();

    /// <summary>
    ///     <para>
    ///         Returns all configured type mapping fragments.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The configured type mapping fragments.</returns>
    public static IEnumerable<IEntityTypeMappingFragment> GetMappingFragments(this ITypeBase typeBase)
        => typeBase.ContainingEntityType.GetMappingFragments();

    /// <summary>
    ///     <para>
    ///         Returns all configured type mapping fragments of the given type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <param name="storeObjectType">The type of store object to get the mapping fragments for.</param>
    /// <returns>The configured type mapping fragments.</returns>
    public static IEnumerable<IReadOnlyEntityTypeMappingFragment> GetMappingFragments(
        this IReadOnlyTypeBase typeBase,
        StoreObjectType storeObjectType)
        => typeBase.ContainingEntityType.GetMappingFragments(storeObjectType);

    /// <summary>
    ///     <para>
    ///         Returns all configured type mapping fragments of the given type.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <param name="storeObjectType">The type of store object to get the mapping fragments for.</param>
    /// <returns>The configured type mapping fragments.</returns>
    public static IEnumerable<IEntityTypeMappingFragment> GetMappingFragments(
        this ITypeBase typeBase,
        StoreObjectType storeObjectType)
        => typeBase.ContainingEntityType.GetMappingFragments(storeObjectType);

    /// <summary>
    ///     <para>
    ///         Returns the type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an type mapping fragment.</returns>
    public static IReadOnlyEntityTypeMappingFragment? FindMappingFragment(
        this IReadOnlyTypeBase typeBase,
        in StoreObjectIdentifier storeObject)
        => typeBase.ContainingEntityType.FindMappingFragment(storeObject);

    /// <summary>
    ///     <para>
    ///         Returns the type mapping for a particular table-like store object.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <param name="storeObject">The identifier of a table-like store object.</param>
    /// <returns>An object that represents an type mapping fragment.</returns>
    public static IEntityTypeMappingFragment? FindMappingFragment(
        this ITypeBase typeBase,
        in StoreObjectIdentifier storeObject)
        => typeBase.ContainingEntityType.FindMappingFragment(storeObject);

    #endregion

    #region Mapping strategy

    /// <summary>
    ///     Gets the mapping strategy for the derived types.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The mapping strategy for the derived types.</returns>
    public static string? GetMappingStrategy(this IReadOnlyTypeBase typeBase)
        => typeBase.ContainingEntityType.GetMappingStrategy();

    #endregion Mapping strategy

    #region Json

    /// <summary>
    ///     Gets a value indicating whether the specified entity is mapped to a JSON column.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>A value indicating whether the associated type is mapped to a JSON column.</returns>
    public static bool IsMappedToJson(this IReadOnlyTypeBase typeBase)
        => !string.IsNullOrEmpty(typeBase.GetContainerColumnName());

    /// <summary>
    ///     Gets the container column name to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to get the container column name for.</param>
    /// <returns>The container column name to which the type is mapped.</returns>
    public static string? GetContainerColumnName(this IReadOnlyTypeBase typeBase)
    {
        var containerColumnName = typeBase.FindAnnotation(RelationalAnnotationNames.ContainerColumnName);
        return containerColumnName != null
            ? (string?)containerColumnName.Value
            : typeBase is IReadOnlyEntityType entityType
                ? entityType.FindOwnership()?.PrincipalEntityType.GetContainerColumnName()
                : ((IReadOnlyComplexType)typeBase).ComplexProperty.DeclaringType.GetContainerColumnName();
    }

    /// <summary>
    ///     Sets the name of the container column to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to set the container column name for.</param>
    /// <param name="columnName">The name to set.</param>
    public static void SetContainerColumnName(this IMutableTypeBase typeBase, string? columnName)
        => typeBase.SetOrRemoveAnnotation(RelationalAnnotationNames.ContainerColumnName, columnName);

    /// <summary>
    ///     Sets the name of the container column to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to set the container column name for.</param>
    /// <param name="columnName">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetContainerColumnName(
        this IConventionTypeBase typeBase,
        string? columnName,
        bool fromDataAnnotation = false)
        => (string?)typeBase.SetAnnotation(RelationalAnnotationNames.ContainerColumnName, columnName, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the container column name.
    /// </summary>
    /// <param name="typeBase">The type to get the container column name configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the container column name.</returns>
    public static ConfigurationSource? GetContainerColumnNameConfigurationSource(this IConventionTypeBase typeBase)
        => typeBase.FindAnnotation(RelationalAnnotationNames.ContainerColumnName)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Gets the column type to use for the container column to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The database column type.</returns>
    public static string? GetContainerColumnType(this IReadOnlyTypeBase typeBase)
        => typeBase.FindAnnotation(RelationalAnnotationNames.ContainerColumnType)?.Value is string columnName
            ? columnName
            : typeBase is IReadOnlyEntityType entityType
                ? entityType.FindOwnership()?.PrincipalEntityType.GetContainerColumnType()
                : ((IReadOnlyComplexType)typeBase).ComplexProperty.DeclaringType.GetContainerColumnType();

    /// <summary>
    ///     Sets the type of the container column to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to set the container column type for.</param>
    /// <param name="columnType">The type to set.</param>
    public static void SetContainerColumnType(this IMutableTypeBase typeBase, string? columnType)
        => typeBase.SetOrRemoveAnnotation(RelationalAnnotationNames.ContainerColumnType, columnType);

    /// <summary>
    ///     Sets the type of the container column to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to set the container column type for.</param>
    /// <param name="columnType">The type to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetContainerColumnType(
        this IConventionTypeBase typeBase,
        string? columnType,
        bool fromDataAnnotation = false)
        => (string?)typeBase.SetAnnotation(RelationalAnnotationNames.ContainerColumnType, columnType, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the container column type.
    /// </summary>
    /// <param name="typeBase">The type to get the container column type configuration source for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the container column type.</returns>
    public static ConfigurationSource? GetContainerColumnTypeConfigurationSource(this IConventionTypeBase typeBase)
        => typeBase.FindAnnotation(RelationalAnnotationNames.ContainerColumnType)
            ?.GetConfigurationSource();

    /// <summary>
    ///     Gets the value of JSON property name used for the given entity mapped to a JSON column.
    /// </summary>
    /// <remarks>
    ///     Unless configured explicitly, navigation name is used.
    /// </remarks>
    /// <param name="typeBase">The type.</param>
    /// <returns>
    ///     The value for the JSON property used to store this type.
    ///     <see langword="null" /> is returned for entities that are not mapped to a JSON column.
    /// </returns>
    public static string? GetJsonPropertyName(this IReadOnlyTypeBase typeBase)
        => (string?)typeBase.FindAnnotation(RelationalAnnotationNames.JsonPropertyName)?.Value
            ?? (!typeBase.IsMappedToJson()
                ? null
                : typeBase is IReadOnlyEntityType entityType
                    ? entityType.FindOwnership()!.GetNavigation(pointsToPrincipal: false)!.Name
                    : ((IReadOnlyComplexType)typeBase).ComplexProperty.Name);

    #endregion
}
