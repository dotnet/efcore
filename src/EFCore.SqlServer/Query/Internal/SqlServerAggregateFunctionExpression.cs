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
public class SqlServerAggregateFunctionExpression : SqlExpression
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
        : base(type, typeMapping)
    {
        Name = name;
        Arguments = arguments.ToList();
        Orderings = orderings;
        IsNullable = nullable;
        ArgumentsPropagateNullability = argumentsPropagateNullability.ToList();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Arguments { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
            ? new SqlServerAggregateFunctionExpression(
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerAggregateFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(
            Name,
            Arguments,
            Orderings,
            IsNullable,
            ArgumentsPropagateNullability,
            Type,
            typeMapping ?? TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerAggregateFunctionExpression Update(
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings)
        => (ReferenceEquals(arguments, Arguments) || arguments.SequenceEqual(Arguments))
            && (ReferenceEquals(orderings, Orderings) || orderings.SequenceEqual(Orderings))
                ? this
                : new SqlServerAggregateFunctionExpression(
                    Name,
                    arguments,
                    orderings,
                    IsNullable,
                    ArgumentsPropagateNullability,
                    Type,
                    TypeMapping);

    // string name,
    //     IReadOnlyList<SqlExpression> arguments,
    // IReadOnlyList<OrderingExpression> orderings,
    // bool nullable,
    //     IEnumerable<bool> argumentsPropagateNullability,
    // Type type,
    //     RelationalTypeMapping? typeMapping)

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(SqlServerAggregateFunctionExpression).GetConstructor(
            [
                typeof(string), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>), typeof(bool),
                typeof(IEnumerable<bool>), typeof(Type), typeof(RelationalTypeMapping)
            ])!,
            Constant(Name),
            Arguments is null
                ? Constant(null, typeof(IEnumerable<SqlExpression>))
                : NewArrayInit(typeof(SqlExpression), initializers: Arguments.Select(a => a.Quote())),
            NewArrayInit(typeof(OrderingExpression), Orderings.Select(o => o.Quote())),
            Constant(IsNullable),
            ArgumentsPropagateNullability is null
                ? Constant(null, typeof(IEnumerable<bool>))
                : NewArrayInit(
                    typeof(bool), initializers: ArgumentsPropagateNullability.Select(n => Constant(n))),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

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

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is SqlServerAggregateFunctionExpression sqlServerFunctionExpression && Equals(sqlServerFunctionExpression);

    private bool Equals(SqlServerAggregateFunctionExpression? other)
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
