// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     An expression that represents a parameter in a SQL tree.
/// </summary>
public sealed class SqlParameterExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlParameterExpression" /> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlParameterExpression(string name, Type type, RelationalTypeMapping? typeMapping)
        : this(name, type.UnwrapNullableType(), type.IsNullableType(), typeMapping)
    {
    }

    private SqlParameterExpression(string name, Type type, bool nullable, RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Name = name;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The bool value indicating if this parameter can have null values.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new SqlParameterExpression(Name, Type, IsNullable, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlParameterExpression).GetConstructor(
                [typeof(string), typeof(Type), typeof(RelationalTypeMapping) ])!, // TODO: There's a dead IsNullable there...
            Constant(Name, typeof(string)),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append("@" + Name);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlParameterExpression sqlParameterExpression
                && Equals(sqlParameterExpression));

    private bool Equals(SqlParameterExpression sqlParameterExpression)
        => base.Equals(sqlParameterExpression)
            && Name == sqlParameterExpression.Name;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name);
}
