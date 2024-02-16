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
    private static ConstructorInfo? _quotingConstructor, _columnInfoQuotingConstructor;

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
    public virtual IReadOnlyList<PathSegment>? Path { get; }

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
        IReadOnlyList<PathSegment>? path = null,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
        : base(alias, "OPENJSON", schema: null, builtIn: true, new[] { jsonExpression })
    {
        if (columnInfos?.Count == 0)
        {
            columnInfos = null;
        }

        Path = path;
        ColumnInfos = columnInfos;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visitedJsonExpression = (SqlExpression)visitor.Visit(JsonExpression);

        PathSegment[]? visitedPath = null;

        if (Path is not null)
        {
            for (var i = 0; i < Path.Count; i++)
            {
                var segment = Path[i];
                PathSegment newSegment;

                if (segment.PropertyName is not null)
                {
                    // PropertyName segments are (currently) constants, nothing to visit.
                    newSegment = segment;
                }
                else
                {
                    var newArrayIndex = (SqlExpression)visitor.Visit(segment.ArrayIndex)!;
                    if (newArrayIndex == segment.ArrayIndex)
                    {
                        newSegment = segment;
                    }
                    else
                    {
                        newSegment = new PathSegment(newArrayIndex);

                        if (visitedPath is null)
                        {
                            visitedPath = new PathSegment[Path.Count];
                            for (var j = 0; j < i; i++)
                            {
                                visitedPath[j] = Path[j];
                            }
                        }
                    }
                }

                if (visitedPath is not null)
                {
                    visitedPath[i] = newSegment;
                }
            }
        }

        return Update(visitedJsonExpression, visitedPath ?? Path, ColumnInfos);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerOpenJsonExpression Update(
        SqlExpression jsonExpression,
        IReadOnlyList<PathSegment>? path,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
    {
        if (columnInfos?.Count == 0)
        {
            columnInfos = null;
        }

        return jsonExpression == JsonExpression
            && (ReferenceEquals(path, Path) || path is not null && Path is not null && path.SequenceEqual(Path))
            && (ReferenceEquals(columnInfos, ColumnInfos)
                || columnInfos is not null && ColumnInfos is not null && columnInfos.SequenceEqual(ColumnInfos))
                ? this
                : new SqlServerOpenJsonExpression(Alias, jsonExpression, path, columnInfos);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
    {
        var newJsonExpression = (SqlExpression)cloningExpressionVisitor.Visit(JsonExpression);
        var clone = new SqlServerOpenJsonExpression(alias!, newJsonExpression, Path, ColumnInfos);

        foreach (var annotation in GetAnnotations())
        {
            clone.AddAnnotation(annotation.Name, annotation.Value);
        }

        return clone;
    }

    /// <inheritdoc />
    public override SqlServerOpenJsonExpression WithAlias(string newAlias)
        => new(newAlias, JsonExpression, Path, ColumnInfos);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlServerOpenJsonExpression).GetConstructor(
            [
                typeof(string),
                typeof(SqlExpression),
                typeof(IReadOnlyList<PathSegment>),
                typeof(IReadOnlyList<ColumnInfo>)
            ])!,
            Constant(Alias, typeof(string)),
            JsonExpression.Quote(),
            Path is null
                ? Constant(null, typeof(IReadOnlyList<PathSegment>))
                : NewArrayInit(typeof(PathSegment), Path.Select(s => s.Quote())),
            ColumnInfos is null
                ? Constant(null, typeof(IReadOnlyList<ColumnInfo>))
                : NewArrayInit(
                    typeof(ColumnInfo), ColumnInfos.Select(
                        ci => New(
                            _columnInfoQuotingConstructor ??= typeof(ColumnInfo).GetConstructor(
                            [
                                typeof(string),
                                typeof(RelationalTypeMapping),
                                typeof(IReadOnlyList<PathSegment>),
                                typeof(bool)
                            ])!,
                            Constant(ci.Name),
                            RelationalExpressionQuotingUtilities.QuoteTypeMapping(ci.TypeMapping),
                            ci.Path is null
                                ? Constant(null, typeof(IReadOnlyList<PathSegment>))
                                : NewArrayInit(typeof(PathSegment), ci.Path.Select(s => s.Quote())),
                            Constant(ci.AsJson)))));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.Visit(JsonExpression);

        if (Path is not null)
        {
            expressionPrinter
                .Append(", '")
                .Append(string.Join(".", Path.Select(e => e.ToString())))
                .Append("'");
        }

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
                    .Append(columnInfo.TypeMapping.StoreType);

                if (columnInfo.Path is not null)
                {
                    expressionPrinter
                        .Append(" '")
                        .Append(string.Join(".", columnInfo.Path.Select(e => e.ToString())))
                        .Append("'");
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

    private bool Equals(SqlServerOpenJsonExpression other)
    {
        if (!base.Equals(other) || ColumnInfos?.Count != other.ColumnInfos?.Count)
        {
            return false;
        }

        if (ReferenceEquals(ColumnInfos, other.ColumnInfos))
        {
            return true;
        }

        for (var i = 0; i < ColumnInfos!.Count; i++)
        {
            var (columnInfo, otherColumnInfo) = (ColumnInfos[i], other.ColumnInfos![i]);

            if (columnInfo.Name != otherColumnInfo.Name
                || !columnInfo.TypeMapping.Equals(otherColumnInfo.TypeMapping)
                || (columnInfo.Path is null != otherColumnInfo.Path is null
                    || (columnInfo.Path is not null
                        && otherColumnInfo.Path is not null
                        && columnInfo.Path.SequenceEqual(otherColumnInfo.Path))))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public readonly record struct ColumnInfo(
        string Name,
        RelationalTypeMapping TypeMapping,
        IReadOnlyList<PathSegment>? Path = null,
        bool AsJson = false);
}
