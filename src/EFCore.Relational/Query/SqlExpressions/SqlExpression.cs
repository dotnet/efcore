// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a scalar value or a SQL token in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public abstract class SqlExpression : Expression, IRelationalQuotableExpression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SqlExpression" /> class.
    /// </summary>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    protected SqlExpression(Type type, RelationalTypeMapping? typeMapping)
    {
        Check.DebugAssert(!type.IsNullableValueType(), "SqlExpression.Type must be reference type or non-nullable value type");

        Type = type;
        TypeMapping = typeMapping;
    }

    /// <inheritdoc />
    public override Type Type { get; }

    /// <summary>
    ///     The <see cref="RelationalTypeMapping" /> associated with this expression.
    /// </summary>
    public virtual RelationalTypeMapping? TypeMapping { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => throw new InvalidOperationException(RelationalStrings.VisitChildrenMustBeOverridden);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public abstract Expression Quote();

    /// <summary>
    ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
    /// </summary>
    /// <param name="expressionPrinter">The expression printer to use.</param>
    protected abstract void Print(ExpressionPrinter expressionPrinter);

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => Print(expressionPrinter);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlExpression sqlExpression
                && Equals(sqlExpression));

    private bool Equals(SqlExpression sqlExpression)
        => Type == sqlExpression.Type
            && ((TypeMapping == null && sqlExpression.TypeMapping == null)
                || TypeMapping?.Equals(sqlExpression.TypeMapping) == true);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Type, TypeMapping);
}
