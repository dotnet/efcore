// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents a parameter query root within the query.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class ParameterQueryRootExpression : QueryRootExpression
{
    /// <summary>
    ///     The parameter expression representing the values for this query root.
    /// </summary>
    public virtual ParameterExpression ParameterExpression { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="ParameterQueryRootExpression" /> class.
    /// </summary>
    /// <param name="asyncQueryProvider">The query provider associated with this query root.</param>
    /// <param name="elementType">The values that this query root represents.</param>
    /// <param name="parameterExpression">The parameter expression representing the values for this query root.</param>
    public ParameterQueryRootExpression(
        IAsyncQueryProvider asyncQueryProvider,
        Type elementType,
        ParameterExpression parameterExpression)
        : base(asyncQueryProvider, elementType)
    {
        ParameterExpression = parameterExpression;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ParameterQueryRootExpression" /> class.
    /// </summary>
    /// <param name="elementType">The values that this query root represents.</param>
    /// <param name="parameterExpression">The parameter expression representing the values for this query root.</param>
    public ParameterQueryRootExpression(Type elementType, ParameterExpression parameterExpression)
        : base(elementType)
    {
        ParameterExpression = parameterExpression;
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new ParameterQueryRootExpression(ElementType, ParameterExpression);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var parameterExpression = (ParameterExpression)visitor.Visit(ParameterExpression);

        return parameterExpression == ParameterExpression
            ? this
            : new ParameterQueryRootExpression(ElementType, parameterExpression);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Visit(ParameterExpression);
}
