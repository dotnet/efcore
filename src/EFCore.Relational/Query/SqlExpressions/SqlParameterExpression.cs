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
        : this(invariantName: name, name: name, type.UnwrapNullableType(), type.IsNullableType(), shouldBeConstantized: false, typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlParameterExpression" /> class.
    /// </summary>
    /// <param name="invariantName">The name of the parameter as it is recorded in <see cref="QueryContext.Parameters" />.</param>
    /// <param name="name">
    ///     The name of the parameter as it will be set on <see cref="DbParameter.ParameterName" /> and inside the SQL as a placeholder
    ///     (before any additional placeholder character prefixing).
    /// </param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="nullable">Whether this parameter can have null values.</param>
    /// <param name="shouldBeConstantized">Whether the user has indicated that this query parameter should be inlined as a constant.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public SqlParameterExpression(
        string invariantName,
        string name,
        Type type,
        bool nullable,
        bool shouldBeConstantized,
        RelationalTypeMapping? typeMapping)
        : base(type.UnwrapNullableType(), typeMapping)
    {
        InvariantName = invariantName;
        Name = name;
        IsNullable = nullable;
        ShouldBeConstantized = shouldBeConstantized;
    }

    /// <summary>
    ///     The name of the parameter as it is recorded in <see cref="QueryContext.Parameters" />.
    /// </summary>
    public string InvariantName { get; }

    /// <summary>
    ///     The name of the parameter as it will be set on <see cref="DbParameter.ParameterName" /> and inside the SQL as a placeholder
    ///     (before any additional placeholder character prefixing).
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The bool value indicating if this parameter can have null values.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    ///     Whether the user has indicated that this query parameter should be inlined as a constant.
    /// </summary>
    public bool ShouldBeConstantized { get; }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new SqlParameterExpression(InvariantName, Name, Type, IsNullable, ShouldBeConstantized, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlParameterExpression).GetConstructor(
                [typeof(string), typeof(Type), typeof(RelationalTypeMapping)])!, // TODO: There's a dead IsNullable there...
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
