// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     An expression that represents a parameter in a SQL tree.
/// </summary>
/// <remarks>
///     This is a simple wrapper around a <see cref="ParameterExpression" /> in the SQL tree.
///     Instances of this type cannot be constructed by application or database provider code. If this is a problem for your
///     application or provider, then please file an issue at
///     <see href="https://github.com/dotnet/efcore">github.com/dotnet/efcore</see>.
/// </remarks>
public sealed class SqlParameterExpression : SqlExpression
{
    private readonly ParameterExpression _parameterExpression;
    private readonly string _name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping? typeMapping)
        : base(parameterExpression.Type.UnwrapNullableType(), typeMapping)
    {
        Check.DebugAssert(parameterExpression.Name != null, "Parameter must have name.");

        _parameterExpression = parameterExpression;
        _name = parameterExpression.Name;
        IsNullable = parameterExpression.Type.IsNullableType();
    }

    /// <summary>
    ///     The name of the parameter.
    /// </summary>
    public string Name
        => _name;

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
        => new SqlParameterExpression(_parameterExpression, typeMapping);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append("@" + _parameterExpression.Name);


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
