// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an unary operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlUnaryExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    private static readonly ISet<ExpressionType> AllowedOperators = new HashSet<ExpressionType>
    {
        ExpressionType.Equal,
        ExpressionType.NotEqual,
        ExpressionType.Convert,
        ExpressionType.Not,
        ExpressionType.Negate
    };

    internal static bool IsValidOperator(ExpressionType operatorType)
        => AllowedOperators.Contains(operatorType);

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlUnaryExpression" /> class.
    /// </summary>
    /// <param name="operatorType">The operator to apply.</param>
    /// <param name="operand">An expression on which operator is applied.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlUnaryExpression(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        if (!IsValidOperator(operatorType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnsupportedOperatorForSqlExpression(
                    operatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
        }

        OperatorType = operatorType;
        Operand = operand;
    }

    /// <summary>
    ///     The operator of this SQL unary operation.
    /// </summary>
    public virtual ExpressionType OperatorType { get; }

    /// <summary>
    ///     The operand of this SQL unary operation.
    /// </summary>
    public virtual SqlExpression Operand { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Operand));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="operand">The <see cref="Operand" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual SqlUnaryExpression Update(SqlExpression operand)
        => operand != Operand
            ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlUnaryExpression).GetConstructor(
                [typeof(ExpressionType), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Constant(OperatorType),
            Operand.Quote(),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        switch (this)
        {
            case { OperatorType: ExpressionType.Convert, TypeMapping: not null }:
                expressionPrinter.Append("CAST(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(" AS ");
                expressionPrinter.Append(TypeMapping.StoreType);
                expressionPrinter.Append(")");
                break;

            case { OperatorType: ExpressionType.Equal }:
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(" IS NULL");
                break;

            case { OperatorType: ExpressionType.NotEqual }:
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(" IS NOT NULL");
                break;

            default:
                expressionPrinter.Append(OperatorType.ToString());
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(")");
                break;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlUnaryExpression sqlUnaryExpression
                && Equals(sqlUnaryExpression));

    private bool Equals(SqlUnaryExpression sqlUnaryExpression)
        => base.Equals(sqlUnaryExpression)
            && OperatorType == sqlUnaryExpression.OperatorType
            && Operand.Equals(sqlUnaryExpression.Operand);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), OperatorType, Operand);
}
