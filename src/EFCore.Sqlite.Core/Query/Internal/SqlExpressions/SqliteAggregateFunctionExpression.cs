// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteAggregateFunctionExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteAggregateFunctionExpression(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Name = name;
        Arguments = arguments.ToList();
        Orderings = orderings;
        IsNullable = nullable;
        ArgumentsPropagateNullability = argumentsPropagateNullability.ToList();
    }

    /// <summary>
    ///     The name of the aggregate SQL function, e.g. <c>group_concat</c>.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The arguments passed to the aggregate function.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Arguments { get; }

    /// <summary>
    ///     The orderings applied to the aggregated input, rendered inside the function call as
    ///     <c>group_concat(value, separator ORDER BY ...)</c>.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <summary>
    ///     Whether the expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     For each argument, whether a <see langword="null" /> value propagates to a <see langword="null" /> result.
    /// </summary>
    public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        SqlExpression[]? arguments = null;
        for (var i = 0; i < Arguments.Count; i++)
        {
            var visitedArgument = (SqlExpression)visitor.Visit(Arguments[i]);
            if (visitedArgument != Arguments[i] && arguments is null)
            {
                arguments = new SqlExpression[Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    arguments[j] = Arguments[j];
                }
            }

            if (arguments is not null)
            {
                arguments[i] = visitedArgument;
            }
        }

        OrderingExpression[]? orderings = null;
        for (var i = 0; i < Orderings.Count; i++)
        {
            var visitedOrdering = (OrderingExpression)visitor.Visit(Orderings[i]);
            if (visitedOrdering != Orderings[i] && orderings is null)
            {
                orderings = new OrderingExpression[Orderings.Count];

                for (var j = 0; j < i; j++)
                {
                    orderings[j] = Orderings[j];
                }
            }

            if (orderings is not null)
            {
                orderings[i] = visitedOrdering;
            }
        }

        return arguments is not null || orderings is not null
            ? new SqliteAggregateFunctionExpression(
                Name,
                arguments ?? Arguments,
                orderings ?? Orderings,
                IsNullable,
                ArgumentsPropagateNullability,
                Type,
                TypeMapping)
            : this;
    }

    /// <summary>
    ///     Applies the given type mapping, returning a new expression.
    /// </summary>
    public virtual SqliteAggregateFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(
            Name,
            Arguments,
            Orderings,
            IsNullable,
            ArgumentsPropagateNullability,
            Type,
            typeMapping ?? TypeMapping);

    /// <summary>
    ///     Returns a new expression with the given arguments and orderings, or this instance if nothing changed.
    /// </summary>
    public virtual SqliteAggregateFunctionExpression Update(
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings)
        => (ReferenceEquals(arguments, Arguments) || arguments.SequenceEqual(Arguments))
            && (ReferenceEquals(orderings, Orderings) || orderings.SequenceEqual(Orderings))
                ? this
                : new SqliteAggregateFunctionExpression(
                    Name,
                    arguments,
                    orderings,
                    IsNullable,
                    ArgumentsPropagateNullability,
                    Type,
                    TypeMapping);

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqliteAggregateFunctionExpression).GetConstructor(
            [
                typeof(string), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>), typeof(bool),
                typeof(IEnumerable<bool>), typeof(Type), typeof(RelationalTypeMapping)
            ])!,
            Constant(Name),
            NewArrayInit(typeof(SqlExpression), initializers: Arguments.Select(a => a.Quote())),
            NewArrayInit(typeof(OrderingExpression), Orderings.Select(o => o.Quote())),
            Constant(IsNullable),
            NewArrayInit(typeof(bool), initializers: ArgumentsPropagateNullability.Select(n => Constant(n))),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);

        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);

        if (Orderings.Count > 0)
        {
            expressionPrinter.Append(" ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is SqliteAggregateFunctionExpression sqliteAggregateFunctionExpression && Equals(sqliteAggregateFunctionExpression);

    private bool Equals(SqliteAggregateFunctionExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && Name == other.Name
            && Arguments.SequenceEqual(other.Arguments)
            && Orderings.SequenceEqual(other.Orderings);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Name);

        for (var i = 0; i < Arguments.Count; i++)
        {
            hash.Add(Arguments[i]);
        }

        for (var i = 0; i < Orderings.Count; i++)
        {
            hash.Add(Orderings[i]);
        }

        return hash.ToHashCode();
    }
}
