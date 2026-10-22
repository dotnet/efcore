// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a DISTINCT in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class DistinctExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="DistinctExpression" /> class.
    /// </summary>
    /// <param name="operand">An expression on which DISTINCT is applied.</param>
    public DistinctExpression(SqlExpression operand)
        : base(operand.Type, operand.TypeMapping)
    {
        Check.NotNull(operand, nameof(operand));

        Operand = operand;
    }

    /// <summary>
    ///     The expression on which DISTINCT is applied.
    /// </summary>
    public virtual SqlExpression Operand { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        return Update((SqlExpression)visitor.Visit(Operand));
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="operand">The <see cref="Operand" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual DistinctExpression Update(SqlExpression operand)
    {
        Check.NotNull(operand, nameof(operand));

        return operand != Operand
            ? new DistinctExpression(operand)
            : this;
    }

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(DistinctExpression).GetConstructor([typeof(SqlExpression)])!,
            Operand.Quote());

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        Check.NotNull(expressionPrinter, nameof(expressionPrinter));

        expressionPrinter.Append("(DISTINCT ");
        expressionPrinter.Visit(Operand);
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is DistinctExpression distinctExpression
                && Equals(distinctExpression));

    private bool Equals(DistinctExpression distinctExpression)
        => base.Equals(distinctExpression)
            && Operand.Equals(distinctExpression.Operand);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Operand);
}
