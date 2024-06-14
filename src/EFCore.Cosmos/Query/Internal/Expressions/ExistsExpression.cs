// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     <para>
///         An expression that represents projecting a SQL EXISTS expression.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/subquery#exists-expression">CosmosDB subqueries</seealso>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ExistsExpression : SqlExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ExistsExpression(SelectExpression subquery, CoreTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Subquery = subquery;
    }

    /// <summary>
    ///     The subquery for which to check for element existence.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual SelectExpression Subquery { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SelectExpression)visitor.Visit(Subquery));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual ExistsExpression Update(SelectExpression subquery)
        => subquery == Subquery
            ? this
            : new ExistsExpression(subquery, TypeMapping);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("EXISTS (");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Visit(Subquery);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ExistsExpression other && Equals(other);

    private bool Equals(ExistsExpression? other)
        => ReferenceEquals(this, other) || (base.Equals(other) && Subquery.Equals(other.Subquery));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Subquery);
}
