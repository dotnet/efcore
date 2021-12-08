// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an array.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class ArrayExpression : SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ArrayExpression" /> class.
    /// </summary>
    /// <param name="values">The values to compare with.</param>
    public ArrayExpression(
        IReadOnlyList<SqlExpression> values)
        : base(typeof(bool), null)
    {
        Values = values;
    }

    /// <summary>
    ///     The values of the array.
    /// </summary>
    public IReadOnlyList<SqlExpression> Values { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        var count = Values.Count;

        expressionPrinter.Append("new [] {");
        for (var i = 0; i < count; i++)
        {
            expressionPrinter.Visit(Values[i]);

            if (i < count - 1)
            {
                expressionPrinter.Append(", ");
            }
        }
        expressionPrinter.Append("}");
    }
}
