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
    ///     The query parameter expression representing the values for this query root.
    /// </summary>
    public virtual QueryParameterExpression QueryParameterExpression { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="ParameterQueryRootExpression" /> class.
    /// </summary>
    /// <param name="asyncQueryProvider">The query provider associated with this query root.</param>
    /// <param name="elementType">The values that this query root represents.</param>
    /// <param name="queryParameterExpression">The parameter expression representing the values for this query root.</param>
    public ParameterQueryRootExpression(
        IAsyncQueryProvider asyncQueryProvider,
        Type elementType,
        QueryParameterExpression queryParameterExpression)
        : base(asyncQueryProvider, elementType)
        => QueryParameterExpression = queryParameterExpression;

    /// <summary>
    ///     Creates a new instance of the <see cref="ParameterQueryRootExpression" /> class.
    /// </summary>
    /// <param name="elementType">The values that this query root represents.</param>
    /// <param name="queryParameterExpression">The query parameter expression representing the values for this query root.</param>
    public ParameterQueryRootExpression(Type elementType, QueryParameterExpression queryParameterExpression)
        : base(elementType)
        => QueryParameterExpression = queryParameterExpression;

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new ParameterQueryRootExpression(ElementType, QueryParameterExpression);

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var queryParameter = (QueryParameterExpression)visitor.Visit(QueryParameterExpression);

        return queryParameter == QueryParameterExpression
            ? this
            : new ParameterQueryRootExpression(ElementType, queryParameter);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Visit(QueryParameterExpression);

    /// <summary>
    ///     This constructor has been obsoleted, use the constructor accepting QueryParameterExpression instead.
    /// </summary>
    [Obsolete("Use the constructor accepting QueryParameterExpression instead.")]
    public ParameterQueryRootExpression(
        IAsyncQueryProvider asyncQueryProvider,
        Type elementType,
        ParameterExpression parameterExpression)
        : this(
            asyncQueryProvider,
            elementType,
            new QueryParameterExpression(
                parameterExpression.Name ?? throw new ArgumentException(CoreStrings.ParameterExpressionMustHaveName(parameterExpression)),
                parameterExpression.Type))
    {
    }

    /// <summary>
    ///     This constructor has been obsoleted, use the constructor accepting QueryParameterExpression instead.
    /// </summary>
    [Obsolete("Use the constructor accepting QueryParameterExpression instead.")]
    public ParameterQueryRootExpression(Type elementType, ParameterExpression parameterExpression)
        : this(
            elementType,
            new QueryParameterExpression(
                parameterExpression.Name ?? throw new ArgumentException(CoreStrings.ParameterExpressionMustHaveName(parameterExpression)),
                parameterExpression.Type))
    {
    }

    /// <summary>
    ///     This constructor has been obsoleted, use QueryParameterExpression instead.
    /// </summary>
    [Obsolete("Use QueryParameterExpression instead.")]
    public virtual ParameterExpression ParameterExpression
        => Parameter(QueryParameterExpression.Type, QueryParameterExpression.Name);
}
