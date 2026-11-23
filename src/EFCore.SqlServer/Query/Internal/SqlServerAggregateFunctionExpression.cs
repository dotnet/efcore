// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerAggregateFunctionExpression : AggregateFunctionExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerAggregateFunctionExpression(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(name, arguments, orderings, nullable, argumentsPropagateNullability, type, typeMapping)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override AggregateFunctionExpression Create(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        => new SqlServerAggregateFunctionExpression(
            name, arguments, orderings, nullable, argumentsPropagateNullability, type, typeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlServerAggregateFunctionExpression).GetConstructor(
            [
                typeof(string), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>), typeof(bool),
                typeof(IEnumerable<bool>), typeof(Type), typeof(RelationalTypeMapping)
            ])!,
            QuoteConstructorArguments());

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);

        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");

        if (Orderings.Count > 0)
        {
            expressionPrinter.Append(" WITHIN GROUP (ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
            expressionPrinter.Append(")");
        }
    }
}
