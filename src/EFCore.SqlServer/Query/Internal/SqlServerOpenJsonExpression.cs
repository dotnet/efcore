// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     An expression that represents a SQL Server OPENJSON function call in a SQL tree.
/// </summary>
/// <remarks>
///     <para>
///         See <see href="https://learn.microsoft.com/sql/t-sql/functions/openjson-transact-sql">OPENJSON (Transact-SQL)</see> for more
///         information and examples.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public class SqlServerOpenJsonExpression : TableValuedFunctionExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression JsonExpression
        => Arguments[0];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Path
        => Arguments.Count == 1 ? null : Arguments[1];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<ColumnInfo>? ColumnInfos { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerOpenJsonExpression(
        string alias,
        SqlExpression jsonExpression,
        SqlExpression? path = null,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
        : base(alias, "OpenJson", schema: null, builtIn: true, path is null ? new[] { jsonExpression } : new[] { jsonExpression, path })
    {
        ColumnInfos = columnInfos;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOpenJsonExpression Update(
        SqlExpression jsonExpression,
        SqlExpression? path,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
        => jsonExpression == JsonExpression
        && path == Path
        && (columnInfos is null ? ColumnInfos is null : ColumnInfos is not null && columnInfos.SequenceEqual(ColumnInfos))
            ? this
            : new SqlServerOpenJsonExpression(Alias, jsonExpression, path, columnInfos);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");

        if (ColumnInfos is not null)
        {
            expressionPrinter.Append(" WITH (");

            for (var i = 0; i < ColumnInfos.Count; i++)
            {
                var columnInfo = ColumnInfos[i];

                if (i > 0)
                {
                    expressionPrinter.Append(", ");
                }

                expressionPrinter
                    .Append(columnInfo.Name)
                    .Append(" ")
                    .Append(columnInfo.StoreType ?? "<UNKNOWN>");

                if (columnInfo.Path is not null)
                {
                    expressionPrinter.Append(" ").Append("'" + columnInfo.Path + "'");
                }

                if (columnInfo.AsJson)
                {
                    expressionPrinter.Append(" AS JSON");
                }
            }

            expressionPrinter.Append(")");
        }

        PrintAnnotations(expressionPrinter);
        expressionPrinter.Append(" AS ");
        expressionPrinter.Append(Alias);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) || (obj is SqlServerOpenJsonExpression openJsonExpression && Equals(openJsonExpression));

    private bool Equals(SqlServerOpenJsonExpression openJsonExpression)
        => base.Equals(openJsonExpression)
            && (ColumnInfos is null
                ? openJsonExpression.ColumnInfos is null
                : openJsonExpression.ColumnInfos is not null && ColumnInfos.SequenceEqual(openJsonExpression.ColumnInfos));

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public readonly record struct ColumnInfo(string Name, string? StoreType, string? Path = null, bool AsJson = false);
}
