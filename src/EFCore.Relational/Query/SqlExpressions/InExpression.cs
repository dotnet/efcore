// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an IN operation in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class InExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructorWithSubquery;
    private static ConstructorInfo? _quotingConstructorWithValues;
    private static ConstructorInfo? _quotingConstructorWithValuesParameter;

    /// <summary>
    ///     Creates a new instance of the <see cref="InExpression" /> class, representing a SQL <c>IN</c> expression with a subquery.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="subquery">A subquery in which the item is searched.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public InExpression(
        SqlExpression item,
        SelectExpression subquery,
        RelationalTypeMapping typeMapping)
        : this(item, subquery, values: null, valuesParameter: null, typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="InExpression" /> class, representing a SQL <c>IN</c> expression with a given list
    ///     of values.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="values">A list of values in which the item is searched.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public InExpression(
        SqlExpression item,
        IReadOnlyList<SqlExpression> values,
        RelationalTypeMapping typeMapping)
        : this(item, subquery: null, values, valuesParameter: null, typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="InExpression" /> class, representing a SQL <c>IN</c> expression with a given
    ///     parameterized list of values.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="valuesParameter">A parameterized list of values in which the item is searched.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public InExpression(
        SqlExpression item,
        SqlParameterExpression valuesParameter,
        RelationalTypeMapping typeMapping)
        : this(item, subquery: null, values: null, valuesParameter, typeMapping)
    {
    }

    private InExpression(
        SqlExpression item,
        SelectExpression? subquery,
        IReadOnlyList<SqlExpression>? values,
        SqlParameterExpression? valuesParameter,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Check.DebugAssert(subquery?.IsMutable != true, "Mutable subquery provided to ExistsExpression");

        Item = item;
        Subquery = subquery;
        Values = values;
        ValuesParameter = valuesParameter;
    }

    /// <summary>
    ///     The item to look into values.
    /// </summary>
    public virtual SqlExpression Item { get; }

    /// <summary>
    ///     The subquery to search the item in.
    /// </summary>
    public virtual SelectExpression? Subquery { get; }

    /// <summary>
    ///     The list of values to search the item in.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression>? Values { get; }

    /// <summary>
    ///     A parameter containing the list of values to search the item in. The parameterized list get expanded to the actual value
    ///     before the query SQL is generated.
    /// </summary>
    public virtual SqlParameterExpression? ValuesParameter { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var item = (SqlExpression)visitor.Visit(Item);
        var subquery = (SelectExpression?)visitor.Visit(Subquery);

        SqlExpression[]? values = null;
        if (Values is not null)
        {
            for (var i = 0; i < Values.Count; i++)
            {
                var value = Values[i];
                var newValue = (SqlExpression)visitor.Visit(value);

                if (newValue != value && values is null)
                {
                    values = new SqlExpression[Values.Count];
                    for (var j = 0; j < i; j++)
                    {
                        values[j] = Values[j];
                    }
                }

                if (values is not null)
                {
                    values[i] = newValue;
                }
            }
        }

        var valuesParameter = (SqlParameterExpression?)visitor.Visit(ValuesParameter);

        return Update(item, subquery, values ?? Values, valuesParameter);
    }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual InExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(Item, Subquery, Values, ValuesParameter, typeMapping);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InExpression Update(SqlExpression item, SelectExpression subquery)
        => Update(item, subquery, values: null, valuesParameter: null);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="values">The <see cref="Values" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InExpression Update(SqlExpression item, IReadOnlyList<SqlExpression> values)
        => Update(item, subquery: null, values, valuesParameter: null);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="valuesParameter">The <see cref="ValuesParameter" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InExpression Update(SqlExpression item, SqlParameterExpression valuesParameter)
        => Update(item, subquery: null, values: null, valuesParameter);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
    /// <param name="values">The <see cref="Values" /> property of the result.</param>
    /// <param name="valuesParameter">The <see cref="ValuesParameter" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InExpression Update(
        SqlExpression item,
        SelectExpression? subquery,
        IReadOnlyList<SqlExpression>? values,
        SqlParameterExpression? valuesParameter)
    {
        if ((subquery is null ? 0 : 1) + (values is null ? 0 : 1) + (valuesParameter is null ? 0 : 1) != 1)
        {
            throw new ArgumentException(
                RelationalStrings.OneOfThreeValuesMustBeSet(nameof(subquery), nameof(values), nameof(valuesParameter)));
        }

        return item == Item && subquery == Subquery && values == Values && valuesParameter == ValuesParameter
            ? this
            : new InExpression(item, subquery, values, valuesParameter, TypeMapping);
    }

    /// <inheritdoc />
    public override Expression Quote()
        => this switch
        {
            { Subquery: not null } => New(
                _quotingConstructorWithSubquery ??= typeof(InExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(SelectExpression), typeof(RelationalTypeMapping)])!,
                Item.Quote(),
                Subquery.Quote(),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping)),

            { Values: not null } => New(
                _quotingConstructorWithValues ??= typeof(InExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(IReadOnlyList<SqlExpression>), typeof(RelationalTypeMapping)])!,
                Item.Quote(),
                NewArrayInit(typeof(SqlExpression), initializers: Values.Select(v => v.Quote())),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping)),

            { ValuesParameter: not null } => New(
                _quotingConstructorWithValuesParameter ??= typeof(InExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(SqlParameterExpression), typeof(RelationalTypeMapping)])!,
                Item.Quote(),
                ValuesParameter.Quote(),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping)),

            _ => throw new UnreachableException()
        };

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Item);
        expressionPrinter.Append(" IN ");
        expressionPrinter.Append("(");

        switch (this)
        {
            case { Subquery: not null }:
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(Subquery);
                }

                break;

            case { Values: not null }:
                for (var i = 0; i < Values.Count; i++)
                {
                    if (i > 0)
                    {
                        expressionPrinter.Append(", ");
                    }

                    expressionPrinter.Visit(Values[i]);
                }

                break;

            case { ValuesParameter: not null }:
                expressionPrinter.Visit(ValuesParameter);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InExpression inExpression
                && Equals(inExpression));

    private bool Equals(InExpression inExpression)
        => base.Equals(inExpression)
            && Item.Equals(inExpression.Item)
            && (Subquery?.Equals(inExpression.Subquery) ?? inExpression.Subquery == null)
            && (ValuesParameter?.Equals(inExpression.ValuesParameter) ?? inExpression.ValuesParameter == null)
            && (ReferenceEquals(Values, inExpression.Values)
                || (Values is not null && inExpression.Values is not null && Values.SequenceEqual(inExpression.Values)));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Item);
        hash.Add(Subquery);
        hash.Add(ValuesParameter);

        if (Values is not null)
        {
            for (var i = 0; i < Values.Count; i++)
            {
                hash.Add(Values[i]);
            }
        }

        return hash.ToHashCode();
    }
}
