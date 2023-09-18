// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

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
        => (IEnumerable<ITableMappingBase>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DefaultMappings)
            ?? Enumerable.Empty<ITableMappingBase>();

    /// <summary>
    ///     Returns the tables to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type to get the table mappings for.</param>
    /// <returns>The tables to which the type is mapped.</returns>
    public static IEnumerable<ITableMapping> GetTableMappings(this ITypeBase typeBase)
        => (IEnumerable<ITableMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableMappings)
            ?? Enumerable.Empty<ITableMapping>();

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
        => (IEnumerable<IViewMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewMappings)
            ?? Enumerable.Empty<IViewMapping>();

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
        => (IEnumerable<ISqlQueryMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.SqlQueryMappings)
            ?? Enumerable.Empty<ISqlQueryMapping>();

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
        => (IEnumerable<IFunctionMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.FunctionMappings)
            ?? Enumerable.Empty<IFunctionMapping>();

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
        => (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.InsertStoredProcedureMappings)
            ?? Enumerable.Empty<IStoredProcedureMapping>();

    /// <summary>
    ///     Returns the delete stored procedures to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The delete stored procedures to which the type is mapped.</returns>
    public static IEnumerable<IStoredProcedureMapping> GetDeleteStoredProcedureMappings(this ITypeBase typeBase)
        => (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.DeleteStoredProcedureMappings)
            ?? Enumerable.Empty<IStoredProcedureMapping>();

    /// <summary>
    ///     Returns the update stored procedures to which the type is mapped.
    /// </summary>
    /// <param name="typeBase">The type.</param>
    /// <returns>The update stored procedures to which the type is mapped.</returns>
    public static IEnumerable<IStoredProcedureMapping> GetUpdateStoredProcedureMappings(this ITypeBase typeBase)
        => (IEnumerable<IStoredProcedureMapping>?)typeBase.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.UpdateStoredProcedureMappings)
            ?? Enumerable.Empty<IStoredProcedureMapping>();

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
        => typeBase is IReadOnlyEntityType entityType
            ? entityType.GetContainerColumnName()
            : ((IReadOnlyComplexType)typeBase).GetContainerColumnName();

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
