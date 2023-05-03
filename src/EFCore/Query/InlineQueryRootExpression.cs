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

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Expression[]? newValues = null;

        for (var i = 0; i < Values.Count; i++)
        {
            var value = Values[i];
            var newValue = visitor.Visit(value);

            if (newValue != value && newValues is null)
            {
                newValues = new Expression[Values.Count];
                for (var j = 0; j < i; j++)
                {
                    newValues[j] = Values[j];
                }
            }

            if (newValues is not null)
            {
                newValues[i] = newValue;
            }
        }

        return newValues is null ? this : new InlineQueryRootExpression(newValues, Type);
    }

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
