// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InExpression : SqlExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InExpression(
        SqlExpression item,
        IReadOnlyList<SqlExpression> values,
        CoreTypeMapping typeMapping)
        : this(item, values, valuesParameter: null, typeMapping)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InExpression(
        SqlExpression item,
        SqlParameterExpression valuesParameter,
        CoreTypeMapping typeMapping)
        : this(item, values: null, valuesParameter, typeMapping)
    {
    }

    private InExpression(
        SqlExpression item,
        IReadOnlyList<SqlExpression>? values,
        SqlParameterExpression? valuesParameter,
        CoreTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
        Item = item;
        Values = values;
        ValuesParameter = valuesParameter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Item { get; }

    /// <summary>
    ///     The list of values to search the item in.
    /// </summary>
    public virtual IReadOnlyList<SqlExpression>? Values { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlParameterExpression? ValuesParameter { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var newItem = (SqlExpression)visitor.Visit(Item);

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

        return Update(newItem, values ?? Values, valuesParameter);
    }

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual InExpression ApplyTypeMapping(CoreTypeMapping? typeMapping)
        => new(Item, Values, ValuesParameter, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InExpression Update(SqlExpression item, IReadOnlyList<SqlExpression> values)
        => item != Item || values != Values
            ? new InExpression(item, values, TypeMapping!)
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InExpression Update(SqlExpression item, SqlParameterExpression valuesParameter)
        => item != Item || ValuesParameter != valuesParameter
            ? new InExpression(item, valuesParameter, TypeMapping!)
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InExpression Update(
        SqlExpression item,
        IReadOnlyList<SqlExpression>? values,
        SqlParameterExpression? valuesParameter)
    {
        if (!(values is null ^ valuesParameter is null))
        {
            throw new ArgumentException(
                CosmosStrings.OneOfTwoValuesMustBeSet(nameof(values), nameof(valuesParameter)));
        }

        return item == Item && values == Values && valuesParameter == ValuesParameter
            ? this
            : new InExpression(item, values, valuesParameter, TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Item);
        expressionPrinter.Append(" IN ");
        expressionPrinter.Append("(");

        switch (this)
        {
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InExpression inExpression
                && Equals(inExpression));

    private bool Equals(InExpression inExpression)
        => base.Equals(inExpression)
            && Item.Equals(inExpression.Item)
            && (ValuesParameter?.Equals(inExpression.ValuesParameter) ?? inExpression.ValuesParameter == null)
            && (ReferenceEquals(Values, inExpression.Values)
                || (Values is not null && inExpression.Values is not null && Values.SequenceEqual(inExpression.Values)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Item);
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
