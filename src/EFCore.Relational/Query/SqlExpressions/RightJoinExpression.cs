// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a RIGHT JOIN in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RightJoinExpression : PredicateJoinExpressionBase
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="RightJoinExpression" /> class.
    /// </summary>
    /// <param name="table">A table source to RIGHT JOIN with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <param name="prunable">Whether this join expression may be pruned if nothing references a column on it.</param>
    public RightJoinExpression(TableExpressionBase table, SqlExpression joinPredicate, bool prunable = false)
        : this(table, joinPredicate, prunable, annotations: null)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal] // For precompiled queries
    public RightJoinExpression(
        TableExpressionBase table,
        SqlExpression joinPredicate,
        bool prunable,
        IReadOnlyDictionary<string, IAnnotation>? annotations = null)
        : base(table, joinPredicate, prunable, annotations)
    {
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="JoinExpressionBase.Table" /> property of the result.</param>
    /// <param name="joinPredicate">The <see cref="PredicateJoinExpressionBase.JoinPredicate" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public override RightJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
        => table != Table || joinPredicate != JoinPredicate
            ? new RightJoinExpression(table, joinPredicate, IsPrunable, Annotations)
            : this;

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="JoinExpressionBase.Table" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public override RightJoinExpression Update(TableExpressionBase table)
        => table != Table
            ? new RightJoinExpression(table, JoinPredicate, IsPrunable, Annotations)
            : this;

    /// <inheritdoc />
    protected override RightJoinExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new(Table, JoinPredicate, IsPrunable, annotations);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(RightJoinExpression).GetConstructor(
                [typeof(TableExpressionBase), typeof(SqlExpression), typeof(bool), typeof(IReadOnlyDictionary<string, IAnnotation>)])!,
            Table.Quote(),
            JoinPredicate.Quote(),
            Constant(IsPrunable),
            RelationalExpressionQuotingUtilities.QuoteAnnotations(Annotations));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("RIGHT JOIN ");
        expressionPrinter.Visit(Table);
        expressionPrinter.Append(" ON ");
        expressionPrinter.Visit(JoinPredicate);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is RightJoinExpression rightJoinExpression
                && Equals(rightJoinExpression));

    private bool Equals(RightJoinExpression rightJoinExpression)
        => base.Equals(rightJoinExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
