// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a window function - an aggregate (or other) function evaluated over a window of rows via an
///         <c>OVER</c> clause, e.g. <c>AVG(Salary) OVER (PARTITION BY DepartmentName)</c>.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[Experimental(EFDiagnostics.ExperimentalApi)]
public class WindowFunctionExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="WindowFunctionExpression" /> class.
    /// </summary>
    /// <param name="function">The function evaluated over the window, e.g. the <c>AVG(Salary)</c> of <c>AVG(Salary) OVER (...)</c>.</param>
    /// <param name="partitions">A list of expressions to partition by.</param>
    /// <param name="orderings">A list of ordering expressions to order by inside the partition.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public WindowFunctionExpression(
        SqlExpression function,
        IReadOnlyList<SqlExpression>? partitions,
        IReadOnlyList<OrderingExpression>? orderings,
        RelationalTypeMapping? typeMapping)
        : base(function.Type, typeMapping ?? function.TypeMapping)
    {
        Function = function;
        Partitions = partitions ?? [];
        Orderings = orderings ?? [];
    }

    /// <summary>
    ///     The function which is evaluated over the window.
    /// </summary>
    public virtual SqlExpression Function { get; }

    /// <summary>
    ///     The list of expressions used in partitioning.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Partitions { get; }

    /// <summary>
    ///     The list of ordering expressions used to order inside the given partition.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var function = (SqlExpression)visitor.Visit(Function);
        var changed = function != Function;

        var partitions = new List<SqlExpression>();
        foreach (var partition in Partitions)
        {
            var newPartition = (SqlExpression)visitor.Visit(partition);
            changed |= newPartition != partition;
            partitions.Add(newPartition);
        }

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in Orderings)
        {
            var newOrdering = (OrderingExpression)visitor.Visit(ordering);
            changed |= newOrdering != ordering;
            orderings.Add(newOrdering);
        }

        return changed
            ? new WindowFunctionExpression(function, partitions, orderings, TypeMapping)
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="function">The <see cref="Function" /> property of the result.</param>
    /// <param name="partitions">The <see cref="Partitions" /> property of the result.</param>
    /// <param name="orderings">The <see cref="Orderings" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual WindowFunctionExpression Update(
        SqlExpression function,
        IReadOnlyList<SqlExpression> partitions,
        IReadOnlyList<OrderingExpression> orderings)
        => function == Function
            && Partitions.SequenceEqual(partitions)
            && Orderings.SequenceEqual(orderings)
                ? this
                : new WindowFunctionExpression(function, partitions, orderings, TypeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(WindowFunctionExpression).GetConstructor(
            [
                typeof(SqlExpression), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>),
                typeof(RelationalTypeMapping)
            ])!,
            Function.Quote(),
            NewArrayInit(typeof(SqlExpression), initializers: Partitions.Select(p => p.Quote())),
            NewArrayInit(typeof(OrderingExpression), initializers: Orderings.Select(o => o.Quote())),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Function);
        expressionPrinter.Append(" OVER(");
        if (Partitions.Any())
        {
            expressionPrinter.Append("PARTITION BY ");
            expressionPrinter.VisitCollection(Partitions);
            if (Orderings.Any())
            {
                expressionPrinter.Append(" ");
            }
        }

        if (Orderings.Any())
        {
            expressionPrinter.Append("ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || (obj is WindowFunctionExpression windowFunctionExpression
                    && Equals(windowFunctionExpression)));

    private bool Equals(WindowFunctionExpression windowFunctionExpression)
        => base.Equals(windowFunctionExpression)
            && Function.Equals(windowFunctionExpression.Function)
            && Partitions.SequenceEqual(windowFunctionExpression.Partitions)
            && Orderings.SequenceEqual(windowFunctionExpression.Orderings);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Function);
        foreach (var partition in Partitions)
        {
            hash.Add(partition);
        }

        foreach (var ordering in Orderings)
        {
            hash.Add(ordering);
        }

        return hash.ToHashCode();
    }
}
