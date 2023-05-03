// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlQuery : TableBase, ISqlQuery
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlQuery(string name, RelationalModel model, string sql)
        : base(name, null, model)
    {
        Sql = sql;
    }

    /// <inheritdoc />
    public virtual string Sql { get; set; }

    /// <inheritdoc />
    public override IColumnBase? FindColumn(IProperty property)
        => property.GetSqlQueryColumnMappings()
            .FirstOrDefault(cm => cm.TableMapping.Table == this)
            ?.Column;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual SqlQueryColumn? FindColumn(string name)
        => (SqlQueryColumn?)base.FindColumn(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ISqlQuery)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<ISqlQueryMapping> ISqlQuery.EntityTypeMappings
    {
        [DebuggerStepThrough]
        get => EntityTypeMappings.Cast<ISqlQueryMapping>();
    }

    /// <inheritdoc />
    IEnumerable<ISqlQueryColumn> ISqlQuery.Columns
    {
        [DebuggerStepThrough]
        get => Columns.Values.Cast<ISqlQueryColumn>();
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    ISqlQueryColumn? ISqlQuery.FindColumn(string name)
        => (ISqlQueryColumn?)base.FindColumn(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    ISqlQueryColumn? ISqlQuery.FindColumn(IProperty property)
        => (ISqlQueryColumn?)FindColumn(property);
}
