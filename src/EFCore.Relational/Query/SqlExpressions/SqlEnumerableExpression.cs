// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an enumerable or group in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlEnumerableExpression : SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SqlEnumerableExpression" /> class.
    /// </summary>
    /// <param name="sqlExpression">The underlying sql expression being enumerated.</param>
    /// <param name="distinct">A value indicating if distinct operator is applied on the enumerable or not.</param>
    /// <param name="orderings">A list of orderings to be applied to the enumerable.</param>
    public SqlEnumerableExpression(SqlExpression sqlExpression, bool distinct, IReadOnlyList<OrderingExpression>? orderings)
        : base(sqlExpression.Type, sqlExpression.TypeMapping)
    {
        SqlExpression = sqlExpression;
        IsDistinct = distinct;
        Orderings = orderings ?? Array.Empty<OrderingExpression>();
    }

    /// <summary>
    ///     The underlying sql expression being enumerated.
    /// </summary>
    public virtual SqlExpression SqlExpression { get; }

    /// <summary>
    ///     The value indicating if distinct operator is applied on the enumerable or not.
    /// </summary>
    public virtual bool IsDistinct { get; }

    /// <summary>
    ///     The list of orderings to be applied to the enumerable.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var sqlExpression = (SqlExpression)visitor.Visit(SqlExpression);
        var orderings = Orderings.Select(e => (OrderingExpression)visitor.Visit(e)).ToList();

        return Update(sqlExpression, orderings);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="sqlExpression">The <see cref="SqlExpression" /> property of the result.</param>
    /// <param name="orderings">The <see cref="Orderings" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual SqlEnumerableExpression Update(SqlExpression sqlExpression, IReadOnlyList<OrderingExpression> orderings)
        => sqlExpression != SqlExpression || !orderings.SequenceEqual(Orderings)
            ? new SqlEnumerableExpression(sqlExpression, IsDistinct, orderings)
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (IsDistinct)
        {
            expressionPrinter.Append("DISTINCT (");
        }

        expressionPrinter.Visit(SqlExpression);

        if (IsDistinct)
        {
            expressionPrinter.Append(")");
        }

        if (Orderings.Count > 0)
        {
            expressionPrinter.Append(" ORDER BY ");
            foreach (var ordering in Orderings)
            {
                expressionPrinter.Visit(ordering);
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlEnumerableExpression sqlEnumerableExpression
                && Equals(sqlEnumerableExpression));

    private bool Equals(SqlEnumerableExpression sqlEnumerableExpression)
        => base.Equals(sqlEnumerableExpression)
            && IsDistinct == sqlEnumerableExpression.IsDistinct
            && SqlExpression.Equals(sqlEnumerableExpression.SqlExpression)
            && Orderings.SequenceEqual(sqlEnumerableExpression.Orderings);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(IsDistinct);
        hash.Add(SqlExpression);
        foreach (var ordering in Orderings)
        {
            hash.Add(ordering);
        }

        return hash.ToHashCode();
    }
}
