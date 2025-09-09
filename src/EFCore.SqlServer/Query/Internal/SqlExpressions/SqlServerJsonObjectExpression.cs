// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlExpressions;

/// <summary>
///     An expression that represents a SQL Server <c>JSON_OBJECT()</c> function call in a SQL tree.
/// </summary>
/// <remarks>
///     <para>
///         See <see href="https://learn.microsoft.com/sql/t-sql/functions/json-object-transact-sql">JSON_OBJECT (Transact-SQL)</see>
///         for more information and examples.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public sealed class SqlServerJsonObjectExpression : SqlFunctionExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerJsonObjectExpression(
        IReadOnlyList<string> propertyNames,
        IReadOnlyList<SqlExpression> propertyValues,
        RelationalTypeMapping typeMapping)
    : base(
        "JSON_OBJECT",
        arguments: propertyValues,
        nullable: false,
        argumentsPropagateNullability: Enumerable.Repeat(false, propertyValues.Count).ToList(),
        typeof(string),
        typeMapping)
    {
        if (propertyNames.Count != propertyValues.Count)
        {
            throw new ArgumentException("The number of property names must match the number of property values.");
        }

        if (typeMapping.StoreType is not "nvarchar(max)")
        {
            throw new ArgumentException("Invalid type mapping for JSON_OBJECT.");
        }

        PropertyNames = propertyNames;
    }

    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     The JSON properties the object consists of.
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; }

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlServerJsonObjectExpression).GetConstructor(
                [
                    typeof(IReadOnlyList<string>),
                    typeof(IReadOnlyList<SqlExpression>),
                    typeof(SqlServerStringTypeMapping),
                ])!,
            NewArrayInit(typeof(string), initializers: PropertyNames.Select(Constant)),
            Arguments is null
                ? Constant(null, typeof(IEnumerable<SqlExpression>))
                : NewArrayInit(typeof(SqlExpression), initializers: Arguments.Select(a => a.Quote())),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("JSON_OBJECT(");

        for (var i = 0; i < PropertyNames.Count; i++)
        {
            var name = PropertyNames[i];
            var value = Arguments![i];
            if (i > 0)
            {
                expressionPrinter.Append(", ");
            }

            expressionPrinter.Append(name).Append(": ");
            expressionPrinter.Visit(value);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlServerJsonObjectExpression other
                && Equals(other));

    private bool Equals(SqlServerJsonObjectExpression other)
        => base.Equals(other) && PropertyNames.SequenceEqual(other.PropertyNames);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());

        foreach (var name in PropertyNames)
        {
            hashCode.Add(name);
        }

        return hashCode.ToHashCode();
    }
}
