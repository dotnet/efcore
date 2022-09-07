// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

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
    /// <summary>
    ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN subquery expression.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="subquery">A subquery in which item is searched.</param>
    /// <param name="negated">A value indicating if the item should be present in the values or absent.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public InExpression(
        SqlExpression item,
        SelectExpression subquery,
        bool negated,
        RelationalTypeMapping typeMapping)
        : this(item, null, subquery, negated, typeMapping)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN values expression.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="values">A list of values in which item is searched.</param>
    /// <param name="negated">A value indicating if the item should be present in the values or absent.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public InExpression(
        SqlExpression item,
        SqlExpression values,
        bool negated,
        RelationalTypeMapping typeMapping)
        : this(item, values, null, negated, typeMapping)
    {
    }

    private InExpression(
        SqlExpression item,
        SqlExpression? values,
        SelectExpression? subquery,
        bool negated,
        RelationalTypeMapping? typeMapping)
        : base(typeof(bool), typeMapping)
    {
#if DEBUG
        if (subquery?.IsMutable() == true)
        {
            throw new InvalidOperationException();
        }
#endif
        Item = item;
        Subquery = subquery;
        Values = values;
        IsNegated = negated;
    }

    /// <summary>
    ///     The item to look into values.
    /// </summary>
    public virtual SqlExpression Item { get; }

    /// <summary>
    ///     The value indicating if item should be present in the values or absent.
    /// </summary>
    public virtual bool IsNegated { get; }

    /// <summary>
    ///     The list of values to search item in.
    /// </summary>
    public virtual SqlExpression? Values { get; }

    /// <summary>
    ///     The subquery to search item in.
    /// </summary>
    public virtual SelectExpression? Subquery { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var item = (SqlExpression)visitor.Visit(Item);
        var subquery = (SelectExpression?)visitor.Visit(Subquery);
        var values = (SqlExpression?)visitor.Visit(Values);

        return Update(item, values, subquery);
    }

    /// <summary>
    ///     Negates this expression by changing presence/absence state indicated by <see cref="IsNegated" />.
    /// </summary>
    /// <returns>An expression which is negated form of this expression.</returns>
    public virtual InExpression Negate()
        => new(Item, Values, Subquery, !IsNegated, TypeMapping);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> property of the result.</param>
    /// <param name="values">The <see cref="Values" /> property of the result.</param>
    /// <param name="subquery">The <see cref="Subquery" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InExpression Update(
        SqlExpression item,
        SqlExpression? values,
        SelectExpression? subquery)
    {
        if (values != null
            && subquery != null)
        {
            throw new ArgumentException(RelationalStrings.EitherOfTwoValuesMustBeNull(nameof(values), nameof(subquery)));
        }

        return item != Item || subquery != Subquery || values != Values
            ? new(item, values, subquery, IsNegated, TypeMapping)
            : this;
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Item);
        expressionPrinter.Append(IsNegated ? " NOT IN " : " IN ");
        expressionPrinter.Append("(");

        if (Subquery != null)
        {
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }
        }
        else if (Values is SqlConstantExpression constantValuesExpression
                 && constantValuesExpression.Value is IEnumerable constantValues)
        {
            var first = true;
            foreach (var item in constantValues)
            {
                if (!first)
                {
                    expressionPrinter.Append(", ");
                }

                first = false;
                expressionPrinter.Append(constantValuesExpression.TypeMapping?.GenerateSqlLiteral(item) ?? item?.ToString() ?? "NULL");
            }
        }
        else
        {
            expressionPrinter.Visit(Values);
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
            && IsNegated.Equals(inExpression.IsNegated)
            && (Values?.Equals(inExpression.Values) ?? inExpression.Values == null)
            && (Subquery?.Equals(inExpression.Subquery) ?? inExpression.Subquery == null);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Item, IsNegated, Values, Subquery);
}
