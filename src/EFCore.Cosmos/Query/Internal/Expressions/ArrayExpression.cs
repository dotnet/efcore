// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     Represents a Cosmos ARRAY() expression, which projects the result of a query as an array (e.g.
///     <c>ARRAY (SELECT VALUE t.name FROM t in p.tags)</c>).
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/subquery#array-expression">
///     CosmosDB array expression
/// </seealso>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public class ArrayExpression(SelectExpression subquery, Type arrayClrType, CoreTypeMapping? arrayTypeMapping = null)
    : SqlExpression(arrayClrType, arrayTypeMapping)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Subquery { get; } = subquery;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ArrayExpression VisitChildren(ExpressionVisitor visitor)
        => visitor.Visit(Subquery) is var newQuery
            && ReferenceEquals(newQuery, Subquery)
                ? this
                : new ArrayExpression((SelectExpression)newQuery, Type, TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("ARRAY (");
        expressionPrinter.Visit(Subquery);
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ArrayExpression other && Equals(other);

    private bool Equals(ArrayExpression? other)
        => ReferenceEquals(this, other) || (base.Equals(other) && Subquery.Equals(other.Subquery));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Subquery);
}
