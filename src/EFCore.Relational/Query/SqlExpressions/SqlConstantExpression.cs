// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a constant in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlConstantExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlConstantExpression" /> class.
    /// </summary>
    /// <param name="value">An <see cref="Object" /> to set the <see cref="Value" /> property equal to.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlConstantExpression(object? value, Type type, RelationalTypeMapping? typeMapping)
        : base(type.UnwrapNullableType(), typeMapping)
        => Value = value;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlConstantExpression" /> class.
    /// </summary>
    /// <param name="value">An <see cref="Object" /> to set the <see cref="Value" /> property equal to.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlConstantExpression(object value, RelationalTypeMapping? typeMapping)
        : this(value, value.GetType(), typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlConstantExpression" /> class.
    /// </summary>
    /// <param name="constantExpression">A <see cref="ConstantExpression" />.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    [Obsolete("Call the constructor accepting a value (and possibly a Type) instead")]
    public SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping? typeMapping)
        : base(constantExpression.Type.UnwrapNullableType(), typeMapping)
    {
    }

    /// <summary>
    ///     The constant value.
    /// </summary>
    public virtual object? Value { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new SqlConstantExpression(Value, Type, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlConstantExpression)
                .GetConstructor([typeof(object), typeof(Type), typeof(RelationalTypeMapping)])!,
            Type.IsValueType
                ? Convert(
                    Constant(Value, Type), typeof(object))
                : Constant(Value, Type),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => Print(Value, expressionPrinter);

    private void Print(object? value, ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(TypeMapping?.GenerateSqlLiteral(value) ?? Value?.ToString() ?? "NULL");

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                && Equals(sqlConstantExpression));

    private bool Equals(SqlConstantExpression sqlConstantExpression)
        => base.Equals(sqlConstantExpression)
            && ValueEquals(Value, sqlConstantExpression.Value);

    private static bool ValueEquals(object? value1, object? value2)
    {
        if (value1 == null)
        {
            return value2 == null;
        }

        if (value1 is IList list1
            && value2 is IList list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (var i = 0; i < list1.Count; i++)
            {
                if (!ValueEquals(list1[i], list2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return value1.Equals(value2);
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Value);
}
