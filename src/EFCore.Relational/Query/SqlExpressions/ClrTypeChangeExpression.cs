// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that changes the CLR type of its operand without changing the generated SQL.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class ClrTypeChangeExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="ClrTypeChangeExpression" /> class.
    /// </summary>
    /// <param name="operand">The expression whose CLR type is changed.</param>
    /// <param name="type">The target CLR type.</param>
    /// <param name="typeMapping">The relational type mapping associated with the expression.</param>
    public ClrTypeChangeExpression(
        SqlExpression operand,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Check.NotNull(operand);

        Operand = operand;
    }

    /// <summary>
    ///     The expression whose CLR type is changed.
    /// </summary>
    public virtual SqlExpression Operand { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor);

        return Update((SqlExpression)visitor.Visit(Operand));
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied operand.
    /// </summary>
    /// <param name="operand">The <see cref="Operand" /> property of the result.</param>
    /// <returns>This expression if the operand did not change, or a new expression otherwise.</returns>
    public virtual ClrTypeChangeExpression Update(SqlExpression operand)
    {
        Check.NotNull(operand);

        return operand != Operand
            ? new ClrTypeChangeExpression(operand, Type, TypeMapping)
            : this;
    }

    /// <summary>
    ///     Applies the supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression with the supplied type mapping.</returns>
    public virtual ClrTypeChangeExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(Operand, Type, typeMapping ?? TypeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(ClrTypeChangeExpression).GetConstructor(
                [typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Operand.Quote(),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        Check.NotNull(expressionPrinter);

        expressionPrinter.Append("CLR_TYPE_CHANGE(");
        expressionPrinter.Visit(Operand);
        expressionPrinter.Append(" AS ").Append(Type.ShortDisplayName()).Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || (obj is ClrTypeChangeExpression clrTypeChangeExpression
                    && Equals(clrTypeChangeExpression)));

    private bool Equals(ClrTypeChangeExpression clrTypeChangeExpression)
        => base.Equals(clrTypeChangeExpression)
            && Operand.Equals(clrTypeChangeExpression.Operand);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Operand);
}
