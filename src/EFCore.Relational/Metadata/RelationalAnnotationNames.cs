// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Names for well-known relational model annotations. Applications should not use these names
///     directly, but should instead use the extension methods on metadata objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalAnnotationNames
{
    /// <summary>
    ///     The prefix used for all relational annotations.
    /// </summary>
    public const string Prefix = "Relational:";

    /// <summary>
    ///     The name for column name annotations.
    /// </summary>
    public const string ColumnName = Prefix + "ColumnName";

    /// <summary>
    ///     The name for column order annotations.
    /// </summary>
    public const string ColumnOrder = Prefix + "ColumnOrder";

    /// <summary>
    ///     The name for column type annotations.
    /// </summary>
    public const string ColumnType = Prefix + "ColumnType";

    /// <summary>
    ///     The name for default value SQL expression annotations.
    /// </summary>
    public const string DefaultValueSql = Prefix + "DefaultValueSql";

    /// <summary>
    ///     The name for computed value SQL expression annotations.
    /// </summary>
    public const string ComputedColumnSql = Prefix + "ComputedColumnSql";

    /// <summary>
    ///     The name for computed column type annotations.
    /// </summary>
    public const string IsStored = Prefix + "IsStored";

    /// <summary>
    ///     The name for default value annotations.
    /// </summary>
    public const string DefaultValue = Prefix + "DefaultValue";

    /// <summary>
    ///     The name for table name annotations.
    /// </summary>
    public const string TableName = Prefix + "TableName";

    /// <summary>
    ///     The name for schema name annotations.
    /// </summary>
    public const string Schema = Prefix + "Schema";

    /// <summary>
    ///     The name for view name annotations.
    /// </summary>
    public const string ViewName = Prefix + "ViewName";

    /// <summary>
    ///     The name for view schema name annotations.
    /// </summary>
    public const string ViewSchema = Prefix + "ViewSchema";

    /// <summary>
    ///     The name for mapped function name annotations.
    /// </summary>
    public const string FunctionName = Prefix + "FunctionName";

    /// <summary>
    ///     The name for mapped delete stored procedure annotations.
    /// </summary>
    public const string DeleteStoredProcedure = Prefix + "DeleteStoredProcedure";

    /// <summary>
    ///     The name for mapped insert stored procedure annotations.
    /// </summary>
    public const string InsertStoredProcedure = Prefix + "InsertStoredProcedure";

    /// <summary>
    ///     The name for mapped update stored procedure annotations.
    /// </summary>
    public const string UpdateStoredProcedure = Prefix + "UpdateStoredProcedure";

    /// <summary>
    ///     The name for mapped sql query annotations.
    /// </summary>
    public const string SqlQuery = Prefix + "SqlQuery";

    /// <summary>
    ///     The name for comment annotations.
    /// </summary>
    public const string Comment = Prefix + "Comment";

    /// <summary>
    ///     The name for collation annotations.
    /// </summary>
    public const string Collation = Prefix + "Collation";

    /// <summary>
    ///     The name for default schema annotations.
    /// </summary>
    public const string DefaultSchema = Prefix + "DefaultSchema";

    /// <summary>
    ///     The name for constraint name annotations.
    /// </summary>
    public const string Name = Prefix + "Name";

    /// <summary>
    ///     The prefix for serialized sequence annotations.
    /// </summary>
    [Obsolete("All sequences are stored in a single annotation now")] // DO NOT REMOVE
    // Used in model snapshot processor. See issue#18557
    public const string SequencePrefix = Prefix + "Sequence:";

    /// <summary>
    ///     The name for sequence annotation.
    /// </summary>
    public const string Sequences = Prefix + "Sequences";

    /// <summary>
    ///     The name for check constraint annotations.
    /// </summary>
    public const string CheckConstraints = Prefix + "CheckConstraints";

    /// <summary>
    ///     The name for filter annotations.
    /// </summary>
    public const string Filter = Prefix + "Filter";

    /// <summary>
    ///     The name for functions annotation.
    /// </summary>
    public const string DbFunctions = Prefix + "DbFunctions";

    /// <summary>
    ///     The name for the annotation containing the maximum length for database identifiers.
    /// </summary>
    public const string MaxIdentifierLength = Prefix + "MaxIdentifierLength";

    /// <summary>
    ///     The name for the annotation containing a flag indicating whether the property is constrained to fixed length values.
    /// </summary>
    public const string IsFixedLength = Prefix + "IsFixedLength";

    /// <summary>
    ///     The name for the annotation containing the definition of a database view.
    /// </summary>
    public const string ViewDefinitionSql = Prefix + "ViewDefinitionSql";

    /// <summary>
    ///     The name for the annotation determining whether the table is excluded from migrations.
    /// </summary>
    public const string IsTableExcludedFromMigrations = Prefix + "IsTableExcludedFromMigrations";

    /// <summary>
    ///     The name for the annotation determining the mapping strategy for inherited properties.
    /// </summary>
    public const string MappingStrategy = Prefix + "MappingStrategy";

    /// <summary>
    ///     The value for the annotation corresponding to the TPC mapping strategy.
    /// </summary>
    public const string TpcMappingStrategy = "TPC";

    /// <summary>
    ///     The value for the annotation corresponding to the TPH mapping strategy.
    /// </summary>
    public const string TphMappingStrategy = "TPH";

    /// <summary>
    ///     The value for the annotation corresponding to the TPT mapping strategy.
    /// </summary>
    public const string TptMappingStrategy = "TPT";

    /// <summary>
    ///     The name for database model annotation.
    /// </summary>
    public const string RelationalModel = Prefix + "RelationalModel";

    /// <summary>
    ///     The name for default mappings annotations.
    /// </summary>
    public const string DefaultMappings = Prefix + "DefaultMappings";

    /// <summary>
    ///     The name for default column mappings annotations.
    /// </summary>
    public const string DefaultColumnMappings = Prefix + "DefaultColumnMappings";

    /// <summary>
    ///     The name for table mappings annotations.
    /// </summary>
    public const string TableMappings = Prefix + "TableMappings";

    /// <summary>
    ///     The name for column mappings annotations.
    /// </summary>
    public const string TableColumnMappings = Prefix + "TableColumnMappings";

    /// <summary>
    ///     The name for view mappings annotations.
    /// </summary>
    public const string ViewMappings = Prefix + "ViewMappings";

    /// <summary>
    ///     The name for view column mappings annotations.
    /// </summary>
    public const string ViewColumnMappings = Prefix + "ViewColumnMappings";

    /// <summary>
    ///     The name for function mappings annotations.
    /// </summary>
    public const string FunctionMappings = Prefix + "FunctionMappings";

    /// <summary>
    ///     The name for function column mappings annotations.
    /// </summary>
    public const string FunctionColumnMappings = Prefix + "FunctionColumnMappings";

    /// <summary>
    ///     The name for insert stored procedure mappings annotations.
    /// </summary>
    public const string InsertStoredProcedureMappings = Prefix + "InsertStoredProcedureMappings";

    /// <summary>
    ///     The name for insert stored procedure result column mappings annotations.
    /// </summary>
    public const string InsertStoredProcedureResultColumnMappings = Prefix + "InsertStoredProcedureResultColumnMappings";

    /// <summary>
    ///     The name for insert stored procedure parameter mappings annotations.
    /// </summary>
    public const string InsertStoredProcedureParameterMappings = Prefix + "InsertStoredProcedureParameterMappings";

    /// <summary>
    ///     The name for delete stored procedure mappings annotations.
    /// </summary>
    public const string DeleteStoredProcedureMappings = Prefix + "DeleteStoredProcedureMappings";

    /// <summary>
    ///     The name for delete stored procedure parameter mappings annotations.
    /// </summary>
    public const string DeleteStoredProcedureParameterMappings = Prefix + "DeleteStoredProcedureParameterMappings";

    /// <summary>
    ///     The name for update stored procedure mappings annotations.
    /// </summary>
    public const string UpdateStoredProcedureMappings = Prefix + "UpdateStoredProcedureMappings";

    /// <summary>
    ///     The name for update stored procedure result column mappings annotations.
    /// </summary>
    public const string UpdateStoredProcedureResultColumnMappings = Prefix + "UpdateStoredProcedureResultColumnMappings";

    /// <summary>
    ///     The name for update stored procedure parameter mappings annotations.
    /// </summary>
    public const string UpdateStoredProcedureParameterMappings = Prefix + "UpdateStoredProcedureParameterMappings";

    /// <summary>
    ///     The name for sql query mappings annotations.
    /// </summary>
    public const string SqlQueryMappings = Prefix + "SqlQueryMappings";

    /// <summary>
    ///     The name for sql query column mappings annotations.
    /// </summary>
    public const string SqlQueryColumnMappings = Prefix + "SqlQueryColumnMappings";

    /// <summary>
    ///     The name for foreign key mappings annotations.
    /// </summary>
    public const string ForeignKeyMappings = Prefix + "ForeignKeyMappings";

    /// <summary>
    ///     The name for table index mappings annotations.
    /// </summary>
    public const string TableIndexMappings = Prefix + "TableIndexMappings";

    /// <summary>
    ///     The name for unique constraint mappings annotations.
    /// </summary>
    public const string UniqueConstraintMappings = Prefix + "UniqueConstraintMappings";

    /// <summary>
    ///     The name for the annotation that contains entity type mapping fragments.
    /// </summary>
    public const string MappingFragments = Prefix + "MappingFragments";

    /// <summary>
    ///     The name for the annotation that contains table-specific facet overrides.
    /// </summary>
    public const string RelationalOverrides = Prefix + "RelationalOverrides";

    /// <summary>
    ///     The name for relational model dependencies annotation.
    /// </summary>
    public const string ModelDependencies = Prefix + "ModelDependencies";

    /// <summary>
    ///     The name for the reader field value getter delegate annotation.
    /// </summary>
    public const string FieldValueGetter = Prefix + "FieldValueGetter";

    /// <summary>
    ///     The name for the annotation specifying container column name to which the object is mapped.
    /// </summary>
    public const string ContainerColumnName = Prefix + "ContainerColumnName";

    /// <summary>
    ///     The name for the annotation specifying container column type mapping.
    /// </summary>
    [Obsolete("Container column mappings are now obtained from IColumnBase.StoreTypeMapping")]
    public const string ContainerColumnTypeMapping = Prefix + "ContainerColumnTypeMapping";

    /// <summary>
    ///     The JSON property name for the element that the property/navigation maps to.
    /// </summary>
    public const string JsonPropertyName = Prefix + "JsonPropertyName";

    /// <summary>
    ///     The name for store (database) type annotations.
    /// </summary>
    public const string StoreType = Prefix + "StoreType";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ISet<string> AllNames = new HashSet<string>
    {
        ColumnName,
        ColumnOrder,
        ColumnType,
        DefaultValueSql,
        ComputedColumnSql,
        IsStored,
        DefaultValue,
        TableName,
        Schema,
        ViewName,
        ViewSchema,
        FunctionName,
        DeleteStoredProcedure,
        InsertStoredProcedure,
        UpdateStoredProcedure,
        SqlQuery,
        Comment,
        Collation,
        DefaultSchema,
        Name,        
        #pragma warning disable CS0618 // Type or member is obsolete
        SequencePrefix,
        #pragma warning restore CS0618 // Type or member is obsolete
        Sequences,
        CheckConstraints,
        Filter,
        DbFunctions,
        MaxIdentifierLength,
        IsFixedLength,
        ViewDefinitionSql,
        IsTableExcludedFromMigrations,
        MappingStrategy,
        RelationalModel,
        DefaultMappings,
        DefaultColumnMappings,
        TableMappings,
        TableColumnMappings,
        ViewMappings,
        ViewColumnMappings,
        FunctionMappings,
        FunctionColumnMappings,
        InsertStoredProcedureMappings,
        InsertStoredProcedureResultColumnMappings,
        InsertStoredProcedureParameterMappings,
        DeleteStoredProcedureMappings,
        DeleteStoredProcedureParameterMappings,
        UpdateStoredProcedureMappings,
        UpdateStoredProcedureResultColumnMappings,
        UpdateStoredProcedureParameterMappings,
        SqlQueryMappings,
        SqlQueryColumnMappings,
        ForeignKeyMappings,
        TableIndexMappings,
        UniqueConstraintMappings,
        MappingFragments,
        RelationalOverrides,
        ModelDependencies,
        FieldValueGetter,
        ContainerColumnName,
        #pragma warning disable CS0618 // Type or member is obsolete
        ContainerColumnTypeMapping,
        #pragma warning restore CS0618 // Type or member is obsolete
        JsonPropertyName,
        StoreType
    };
}
