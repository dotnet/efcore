// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     <para>
///         An expression that represents projecting a scalar SQL value from a subquery.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ScalarSubqueryExpression : SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ScalarSubqueryExpression" /> class.
    /// </summary>
    /// <param name="subquery">A subquery projecting single row with a single scalar projection.</param>
    public ScalarSubqueryExpression(SelectExpression subquery)
        : this(
            subquery,
            subquery.Projection[0].Expression is SqlExpression sqlExpression
                ? sqlExpression.TypeMapping
                : throw new UnreachableException("Can't construct scalar subquery over SelectExpresison with non-SqlExpression projection"))
    {
        Subquery = subquery;
    }

    private ScalarSubqueryExpression(SelectExpression subquery, CoreTypeMapping? typeMapping)
        : base(subquery.Projection[0].Type, typeMapping)
    {
        Subquery = subquery;
    }

    /// <summary>
    ///     The subquery projecting single row with single scalar projection.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual SelectExpression Subquery { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlExpression ApplyTypeMapping(CoreTypeMapping? typeMapping)
        => new ScalarSubqueryExpression(Subquery, typeMapping);

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
    public virtual ScalarSubqueryExpression Update(SelectExpression subquery)
        => subquery == Subquery
            ? this
            : new ScalarSubqueryExpression(subquery);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("(");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Visit(Subquery);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ScalarSubqueryExpression other && Equals(other);

    private bool Equals(ScalarSubqueryExpression? other)
        => ReferenceEquals(this, other) || (base.Equals(other) && Subquery.Equals(other.Subquery));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Subquery);
}
