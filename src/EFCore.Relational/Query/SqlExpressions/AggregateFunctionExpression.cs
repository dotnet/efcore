// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an aggregate function call in a SQL tree, optionally with an ordering
///         over its input rows.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class AggregateFunctionExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="AggregateFunctionExpression" /> class.
    /// </summary>
    /// <param name="name">The name of the aggregate function.</param>
    /// <param name="arguments">The arguments of the aggregate function.</param>
    /// <param name="orderings">The orderings applied to the input rows of the aggregate function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="argumentsPropagateNullability">A list of bool values indicating whether individual arguments propagate null to the result.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public AggregateFunctionExpression(
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
    ///     The name of the aggregate function.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The list of arguments of this aggregate function.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression> Arguments { get; }

    /// <summary>
    ///     The list of orderings applied to the input rows of this aggregate function.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <summary>
    ///     A bool value indicating if the function can return null result.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     A list of bool values indicating whether individual argument propagate null to the result.
    /// </summary>
    public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; }

    /// <summary>
    ///     Creates a new expression of the same kind as this one with the given values. Derived types override this
    ///     to preserve their own type and any additional state.
    /// </summary>
    /// <param name="name">The name of the aggregate function.</param>
    /// <param name="arguments">The arguments of the aggregate function.</param>
    /// <param name="orderings">The orderings applied to the input rows of the aggregate function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="argumentsPropagateNullability">A list of bool values indicating whether individual arguments propagate null to the result.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>The newly created expression.</returns>
    protected virtual AggregateFunctionExpression Create(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        => new(name, arguments, orderings, nullable, argumentsPropagateNullability, type, typeMapping);

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
            ? Create(
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
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual AggregateFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => Create(
            Name,
            Arguments,
            Orderings,
            IsNullable,
            ArgumentsPropagateNullability,
            Type,
            typeMapping ?? TypeMapping);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="arguments">The <see cref="Arguments" /> property of the result.</param>
    /// <param name="orderings">The <see cref="Orderings" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual AggregateFunctionExpression Update(
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings)
        => (ReferenceEquals(arguments, Arguments) || arguments.SequenceEqual(Arguments))
            && (ReferenceEquals(orderings, Orderings) || orderings.SequenceEqual(Orderings))
                ? this
                : Create(
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
            _quotingConstructor ??= typeof(AggregateFunctionExpression).GetConstructor(
            [
                typeof(string), typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>), typeof(bool),
                typeof(IEnumerable<bool>), typeof(Type), typeof(RelationalTypeMapping)
            ])!,
            QuoteConstructorArguments());

    /// <summary>
    ///     Quotes the arguments of the <see cref="AggregateFunctionExpression" /> constructor, in order; derived types with the same
    ///     constructor signature can use this to implement <see cref="Quote" />.
    /// </summary>
    /// <returns>The quoted constructor arguments.</returns>
    protected virtual IEnumerable<Expression> QuoteConstructorArguments()
        =>
        [
            Constant(Name),
            NewArrayInit(typeof(SqlExpression), initializers: Arguments.Select(a => a.Quote())),
            NewArrayInit(typeof(OrderingExpression), Orderings.Select(o => o.Quote())),
            Constant(IsNullable),
            NewArrayInit(typeof(bool), initializers: ArgumentsPropagateNullability.Select(n => Constant(n))),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping)
        ];

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
        => obj is AggregateFunctionExpression aggregateFunctionExpression && Equals(aggregateFunctionExpression);

    private bool Equals(AggregateFunctionExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && GetType() == other.GetType()
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
