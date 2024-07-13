// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an IS DISTINCT FROM in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class IsDistinctFromExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="IsDistinctFromExpression" /> class.
    /// </summary>
    /// <param name="left">An expression which is left operand.</param>
    /// <param name="right">An expression which is right operand.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public IsDistinctFromExpression(
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The left operand.
    /// </summary>
    public virtual SqlExpression Left { get; }

    /// <summary>
    ///     The right operand.
    /// </summary>
    public virtual SqlExpression Right { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var left = (SqlExpression)visitor.Visit(Left);
        var right = (SqlExpression)visitor.Visit(Right);

        return Update(left, right);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="left">The <see cref="Left" /> property of the result.</param>
    /// <param name="right">The <see cref="Right" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual IsDistinctFromExpression Update(SqlExpression left, SqlExpression right)
        => left != Left || right != Right
            ? new IsDistinctFromExpression(left, right, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(IsDistinctFromExpression).GetConstructor(
                [typeof(SqlExpression), typeof(SqlExpression), typeof(RelationalTypeMapping)])!,
            Left.Quote(),
            Right.Quote(),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        var requiresBrackets = RequiresBrackets(Left);

        if (requiresBrackets)
        {
            expressionPrinter.Append("(");
        }

        expressionPrinter.Visit(Left);

        if (requiresBrackets)
        {
            expressionPrinter.Append(")");
        }

        expressionPrinter.Append(" IS DISTINCT FROM ");

        requiresBrackets = RequiresBrackets(Right);

        if (requiresBrackets)
        {
            expressionPrinter.Append("(");
        }

        expressionPrinter.Visit(Right);

        if (requiresBrackets)
        {
            expressionPrinter.Append(")");
        }

        static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression or LikeExpression or InExpression;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is IsDistinctFromExpression isExpression
                && Equals(isExpression));

    private bool Equals(IsDistinctFromExpression isExpression)
        => base.Equals(isExpression)
            && Left.Equals(isExpression.Left)
            && Right.Equals(isExpression.Right);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Left, Right);
}
