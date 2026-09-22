// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a COLLATE in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class CollateExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="CollateExpression" /> class.
    /// </summary>
    /// <param name="operand">An expression on which collation is applied.</param>
    /// <param name="collation">A collation value to use.</param>
    public CollateExpression(SqlExpression operand, string collation)
        : base(operand.Type, operand.TypeMapping)
    {
        Operand = operand;
        Collation = collation;
    }

    /// <summary>
    ///     The expression on which collation is applied.
    /// </summary>
    public virtual SqlExpression Operand { get; }

    /// <summary>
    ///     The collation value to use.
    /// </summary>
    public virtual string Collation { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Operand));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="operand">The <see cref="Operand" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual CollateExpression Update(SqlExpression operand)
        => operand != Operand
            ? new CollateExpression(operand, Collation)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(CollateExpression).GetConstructor([typeof(SqlExpression), typeof(string)])!,
            Operand.Quote(),
            Constant(Collation));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Operand);
        expressionPrinter
            .Append(" COLLATE ")
            .Append(Collation);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CollateExpression collateExpression
                && Equals(collateExpression));

    private bool Equals(CollateExpression collateExpression)
        => base.Equals(collateExpression)
            && Operand.Equals(collateExpression.Operand)
            && Collation.Equals(collateExpression.Collation, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Operand, Collation);
}
