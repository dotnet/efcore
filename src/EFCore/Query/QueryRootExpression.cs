// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents a query root in query expression.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public abstract class QueryRootExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="QueryRootExpression" /> class with associated query provider.
    /// </summary>
    /// <param name="asyncQueryProvider">The query provider associated with this query root.</param>
    /// <param name="elementType">The element type this query root represents.</param>
    protected QueryRootExpression(IAsyncQueryProvider asyncQueryProvider, Type elementType)
    {
        QueryProvider = asyncQueryProvider;
        ElementType = elementType;
        Type = typeof(IQueryable<>).MakeGenericType(elementType);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="EntityQueryRootExpression" /> class without any query provider.
    /// </summary>
    /// <param name="elementType">The element type this query root represents.</param>
    protected QueryRootExpression(Type elementType)
    {
        ElementType = elementType;
        Type = typeof(IQueryable<>).MakeGenericType(elementType);
    }

    /// <summary>
    ///     The query provider associated with this query root.
    /// </summary>
    public virtual IAsyncQueryProvider? QueryProvider { get; }

    /// <summary>
    ///     The element type represented by this query root.
    /// </summary>
    public virtual Type ElementType { get; }

    /// <summary>
    ///     Detaches the associated query provider from this query root expression.
    /// </summary>
    /// <returns>A new query root expression without query provider.</returns>
    public abstract Expression DetachQueryProvider();

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    public override bool CanReduce
        => false;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <summary>
    ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
    /// </summary>
    /// <param name="expressionPrinter">The expression printer to use.</param>
    protected abstract void Print(ExpressionPrinter expressionPrinter);

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => Print(expressionPrinter);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is QueryRootExpression queryRootExpression
                && ElementType == queryRootExpression.ElementType);

    /// <inheritdoc />
    public override int GetHashCode()
        => ElementType.GetHashCode();
}
