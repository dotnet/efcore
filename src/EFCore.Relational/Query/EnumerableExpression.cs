// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents an enumerable or group translated from chain over a grouping element.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class EnumerableExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="EnumerableExpression" /> class.
    /// </summary>
    /// <param name="selector">The underlying sql expression being enumerated.</param>
    public EnumerableExpression(Expression selector)
    {
        Selector = selector;
        IsDistinct = false;
        Predicate = null;
        Orderings = new List<OrderingExpression>();
    }

    private EnumerableExpression(
        Expression selector,
        bool distinct,
        SqlExpression? predicate,
        IReadOnlyList<OrderingExpression> orderings)
    {
        Selector = selector;
        IsDistinct = distinct;
        Predicate = predicate;
        Orderings = orderings;
    }

    /// <summary>
    ///     The underlying expression being enumerated.
    /// </summary>
    public virtual Expression Selector { get; }

    /// <summary>
    ///     The value indicating if distinct operator is applied on the enumerable or not.
    /// </summary>
    public virtual bool IsDistinct { get; }

    /// <summary>
    ///     The value indicating any predicate applied on the enumerable.
    /// </summary>
    public virtual SqlExpression? Predicate { get; }

    /// <summary>
    ///     The list of orderings to be applied to the enumerable.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <summary>
    ///     Applies new selector to the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <returns>The new expression with specified component updated.</returns>
    public virtual EnumerableExpression ApplySelector(Expression expression)
        => new(expression, IsDistinct, Predicate, Orderings);

    /// <summary>
    ///     Applies DISTINCT operator to the selector of the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <returns>The new expression with specified component updated.</returns>
    public virtual EnumerableExpression ApplyDistinct()
        => new(Selector, distinct: true, Predicate, Orderings);

    /// <summary>
    ///     Applies filter predicate to the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <param name="sqlExpression">An expression to use for filtering.</param>
    /// <returns>The new expression with specified component updated.</returns>
    public virtual EnumerableExpression ApplyPredicate(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlConstantExpression { Value: true })
        {
            return this;
        }

        var predicate = Predicate == null
            ? sqlExpression
            : new SqlBinaryExpression(
                ExpressionType.AndAlso,
                Predicate,
                sqlExpression,
                typeof(bool),
                sqlExpression.TypeMapping);

        return new EnumerableExpression(Selector, IsDistinct, predicate, Orderings);
    }

    /// <summary>
    ///     Applies ordering to the <see cref="EnumerableExpression" />. This overwrites any previous ordering specified.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    /// <returns>The new expression with specified component updated.</returns>
    public virtual EnumerableExpression ApplyOrdering(OrderingExpression orderingExpression)
    {
        var orderings = new List<OrderingExpression>();
        AppendOrdering(orderings, orderingExpression);

        return new EnumerableExpression(Selector, IsDistinct, Predicate, orderings);
    }

    /// <summary>
    ///     Appends ordering to the existing orderings of the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    /// <returns>The new expression with specified component updated.</returns>
    public virtual EnumerableExpression AppendOrdering(OrderingExpression orderingExpression)
    {
        var orderings = Orderings.ToList();
        AppendOrdering(orderings, orderingExpression);

        return new EnumerableExpression(Selector, IsDistinct, Predicate, orderings);
    }

    private static void AppendOrdering(List<OrderingExpression> orderings, OrderingExpression orderingExpression)
    {
        if (!orderings.Any(o => o.Expression.Equals(orderingExpression.Expression)))
        {
            orderings.Add(orderingExpression.Update(orderingExpression.Expression));
        }
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => throw new InvalidOperationException(
            CoreStrings.VisitIsNotAllowed($"{nameof(EnumerableExpression)}.{nameof(VisitChildren)}"));

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => typeof(IEnumerable<>).MakeGenericType(Selector.Type);

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine(nameof(EnumerableExpression) + ":");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("Selector: ");
            expressionPrinter.Visit(Selector);
            expressionPrinter.AppendLine();
            if (IsDistinct)
            {
                expressionPrinter.AppendLine($"IsDistinct: {IsDistinct}");
            }

            if (Predicate != null)
            {
                expressionPrinter.Append("Predicate: ");
                expressionPrinter.Visit(Predicate);
                expressionPrinter.AppendLine();
            }

            if (Orderings.Count > 0)
            {
                expressionPrinter.Append("Orderings: ");
                expressionPrinter.VisitCollection(Orderings);
                expressionPrinter.AppendLine();
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is EnumerableExpression enumerableExpression
                && Equals(enumerableExpression));

    private bool Equals(EnumerableExpression enumerableExpression)
        => IsDistinct == enumerableExpression.IsDistinct
            && (Predicate == null
                ? enumerableExpression.Predicate == null
                : Predicate.Equals(enumerableExpression.Predicate))
            && ExpressionEqualityComparer.Instance.Equals(Selector, enumerableExpression.Selector)
            && Orderings.SequenceEqual(enumerableExpression.Orderings);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsDistinct);
        hashCode.Add(Selector);
        hashCode.Add(Predicate);
        foreach (var ordering in Orderings)
        {
            hashCode.Add(ordering);
        }

        return hashCode.ToHashCode();
    }
}
