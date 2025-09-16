// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlQueryMapping : TableMappingBase<SqlQueryColumnMapping>, ISqlQueryMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlQueryMapping(
        IEntityType entityType,
        SqlQuery sqlQuery,
        bool? includesDerivedTypes)
        : base(entityType, sqlQuery, includesDerivedTypes)
    {
    }

    /// <inheritdoc />
    public virtual bool IsDefaultSqlQueryMapping { get; set; }

    /// <inheritdoc />
    public virtual ISqlQuery SqlQuery
        => (ISqlQuery)base.Table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ISqlQueryMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<ISqlQueryColumnMapping> ISqlQueryMapping.ColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }
}
