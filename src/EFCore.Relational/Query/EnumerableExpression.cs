// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
    private readonly List<OrderingExpression> _orderings = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="EnumerableExpression" /> class.
    /// </summary>
    /// <param name="selector">The underlying sql expression being enumerated.</param>
    public EnumerableExpression(Expression selector)
    {
        Selector = selector;
    }

    /// <summary>
    ///     The underlying expression being enumerated.
    /// </summary>
    public virtual Expression Selector { get; private set; }

    /// <summary>
    ///     The value indicating if distinct operator is applied on the enumerable or not.
    /// </summary>
    public virtual bool IsDistinct { get; private set; }

    /// <summary>
    ///     The value indicating any predicate applied on the enumerable.
    /// </summary>
    public virtual SqlExpression? Predicate { get; private set;  }

    /// <summary>
    ///     The list of orderings to be applied to the enumerable.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings => _orderings;


    /// <summary>
    ///     Applies new selector to the <see cref="EnumerableExpression" />.
    /// </summary>
    public virtual void ApplySelector(Expression expression)
    {
        Selector = expression;
    }

    /// <summary>
    ///     Applies DISTINCT operator to the selector of the <see cref="EnumerableExpression" />.
    /// </summary>
    public virtual void ApplyDistinct()
    {
        IsDistinct = true;
    }

    /// <summary>
    ///     Applies filter predicate to the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <param name="sqlExpression">An expression to use for filtering.</param>
    public virtual void ApplyPredicate(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlConstantExpression sqlConstant
            && sqlConstant.Value is bool boolValue
            && boolValue)
        {
            return;
        }

        Predicate = Predicate == null
            ? sqlExpression
            : new SqlBinaryExpression(
                ExpressionType.AndAlso,
                Predicate,
                sqlExpression,
                typeof(bool),
                sqlExpression.TypeMapping);
    }

    /// <summary>
    ///     Applies ordering to the <see cref="EnumerableExpression" />. This overwrites any previous ordering specified.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    public virtual void ApplyOrdering(OrderingExpression orderingExpression)
    {
        _orderings.Clear();
        AppendOrdering(orderingExpression);
    }

    /// <summary>
    ///     Appends ordering to the existing orderings of the <see cref="EnumerableExpression" />.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    public virtual void AppendOrdering(OrderingExpression orderingExpression)
    {
        if (!_orderings.Any(o => o.Expression.Equals(orderingExpression.Expression)))
        {
            _orderings.Add(orderingExpression.Update(orderingExpression.Expression));
        }
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => throw new InvalidOperationException(
            CoreStrings.VisitIsNotAllowed($"{nameof(EnumerableExpression)}.{nameof(VisitChildren)}"));

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type => typeof(IEnumerable<>).MakeGenericType(Selector.Type);

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
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    /// <inheritdoc />
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}
