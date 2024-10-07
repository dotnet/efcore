// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public class ObjectBinaryExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ObjectBinaryExpression(ExpressionType operatorType, Expression left, Expression right, Type type)
    {
        if (!IsValidOperator(operatorType))
        {
            throw new InvalidOperationException(
                CosmosStrings.UnsupportedOperatorForSqlExpression(
                    operatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
        }

        Type = type;
        OperatorType = operatorType;
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ExpressionType OperatorType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Left { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Right { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var left = visitor.Visit(Left);
        var right = (Expression)visitor.Visit(Right);

        return Update(left, right);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ObjectBinaryExpression Update(Expression left, Expression right)
        => left != Left || right != Right
            ? new ObjectBinaryExpression(OperatorType, left, right, Type)
            : this;

    internal static bool IsValidOperator(ExpressionType operatorType)
        => operatorType is ExpressionType.Coalesce;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Print(ExpressionPrinter expressionPrinter)
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

        static bool RequiresBrackets(Expression expression)
            => expression is ObjectBinaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ObjectBinaryExpression other
                && Equals(other));

    private bool Equals(ObjectBinaryExpression other)
        => Type == other.Type
            && OperatorType == other.OperatorType
            && Left.Equals(other.Left)
            && Right.Equals(other.Right);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(Type, OperatorType, Left, Right);
}
