// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents an inline query root within the query (e.g. <c>new[] { 1, 2, 3 }</c>).
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
public class InlineQueryRootExpression : QueryRootExpression
{
    /// <summary>
    ///     The values contained in this query root.
    /// </summary>
    public virtual IReadOnlyList<Expression> Values { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="InlineQueryRootExpression" /> class.
    /// </summary>
    /// <param name="asyncQueryProvider">The query provider associated with this query root.</param>
    /// <param name="values">The values contained in this query root.</param>
    /// <param name="elementType">The element type this query root represents.</param>
    public InlineQueryRootExpression(IAsyncQueryProvider asyncQueryProvider, IReadOnlyList<Expression> values, Type elementType)
        : base(asyncQueryProvider, elementType)
    {
        Values = values;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="InlineQueryRootExpression" /> class.
    /// </summary>
    /// <param name="values">An expression containing the values that this query root represents.</param>
    /// <param name="elementType">The element type this query root represents.</param>
    public InlineQueryRootExpression(IReadOnlyList<Expression> values, Type elementType)
        : base(elementType)
    {
        Values = values;
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new InlineQueryRootExpression(Values, ElementType);

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="values">The <see cref="Values" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual InlineQueryRootExpression Update(IReadOnlyList<Expression> values)
        => ReferenceEquals(values, Values) || values.SequenceEqual(Values)
            ? this
            : new InlineQueryRootExpression(values, ElementType);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => visitor.Visit(Values) is var visitedValues
            && ReferenceEquals(visitedValues, Values)
                ? this
                : Update(visitedValues);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("[");

        for (var i = 0; i < Values.Count; i++)
        {
            if (i > 0)
            {
                expressionPrinter.Append(",");
            }

            expressionPrinter.Visit(Values[i]);
        }

        expressionPrinter.Append("]");
    }
}
