// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a binary operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlBinaryExpression : SqlExpression
{
    private static readonly ISet<ExpressionType> AllowedOperators = new HashSet<ExpressionType>
    {
        ExpressionType.Add,
        ExpressionType.Subtract,
        ExpressionType.Multiply,
        ExpressionType.Divide,
        ExpressionType.Modulo,
        //ExpressionType.Power,
        ExpressionType.And,
        ExpressionType.AndAlso,
        ExpressionType.Or,
        ExpressionType.OrElse,
        ExpressionType.LessThan,
        ExpressionType.LessThanOrEqual,
        ExpressionType.GreaterThan,
        ExpressionType.GreaterThanOrEqual,
        ExpressionType.Equal,
        ExpressionType.NotEqual
        //ExpressionType.ExclusiveOr,
        //ExpressionType.ArrayIndex,
        //ExpressionType.RightShift,
        //ExpressionType.LeftShift,
    };

    private static ConstructorInfo? _quotingConstructor;

    internal static bool IsValidOperator(ExpressionType operatorType)
        => AllowedOperators.Contains(operatorType);

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlBinaryExpression" /> class.
    /// </summary>
    /// <param name="operatorType">The operator to apply.</param>
    /// <param name="left">An expression which is left operand.</param>
    /// <param name="right">An expression which is right operand.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlBinaryExpression(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        if (!IsValidOperator(operatorType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnsupportedOperatorForSqlExpression(
                    operatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
        }

        OperatorType = operatorType;
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The operator of this SQL binary operation.
    /// </summary>
    public virtual ExpressionType OperatorType { get; }

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
    public virtual SqlBinaryExpression Update(SqlExpression left, SqlExpression right)
        => left != Left || right != Right
            ? new SqlBinaryExpression(OperatorType, left, right, Type, TypeMapping)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlBinaryExpression).GetConstructor(
                [typeof(ExpressionType), typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Constant(OperatorType),
            Left.Quote(),
            Right.Quote(),
            Constant(Type),
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

        expressionPrinter.Append(expressionPrinter.GenerateBinaryOperator(OperatorType));

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
            => expression is SqlBinaryExpression or LikeExpression;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlBinaryExpression sqlBinaryExpression
                && Equals(sqlBinaryExpression));

    private bool Equals(SqlBinaryExpression sqlBinaryExpression)
        => base.Equals(sqlBinaryExpression)
            && OperatorType == sqlBinaryExpression.OperatorType
            && Left.Equals(sqlBinaryExpression.Left)
            && Right.Equals(sqlBinaryExpression.Right);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), OperatorType, Left, Right);
}
