// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class SqlParameterExpression: SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SqlParameterExpression" /> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> associated with the expression.</param>
    public SqlParameterExpression(string name, Type type, CoreTypeMapping? typeMapping)
        : this(invariantName: name, name: name, type.UnwrapNullableType(), type.IsNullableType(), shouldBeConstantized: false, typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlParameterExpression" /> class.
    /// </summary>
    /// <param name="invariantName">The name of the parameter as it is recorded in <see cref="QueryContext.ParameterValues" />.</param>
    /// <param name="name">
    ///     The name of the parameter as it will be set on <see cref="DbParameter.ParameterName" /> and inside the SQL as a placeholder
    ///     (before any additional placeholder character prefixing).
    /// </param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="nullable">Whether this parameter can have null values.</param>
    /// <param name="shouldBeConstantized">Whether the user has indicated that this query parameter should be inlined as a constant.</param>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> associated with the expression.</param>
    public SqlParameterExpression(
        string invariantName,
        string name,
        Type type,
        bool nullable,
        bool shouldBeConstantized,
        CoreTypeMapping? typeMapping)
        : base(type.UnwrapNullableType(), typeMapping)
    {
        InvariantName = invariantName;
        Name = name;
        IsNullable = nullable;
        ShouldBeConstantized = shouldBeConstantized;
    }

    /// <summary>
    ///     The name of the parameter as it is recorded in <see cref="QueryContext.ParameterValues" />.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression ApplyTypeMapping(CoreTypeMapping? typeMapping)
        => new SqlParameterExpression(InvariantName, Name, Type, IsNullable, ShouldBeConstantized, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append("@" + Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlParameterExpression sqlParameterExpression
                && Equals(sqlParameterExpression));

    private bool Equals(SqlParameterExpression sqlParameterExpression)
        => base.Equals(sqlParameterExpression) && Name == sqlParameterExpression.Name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name);
}
