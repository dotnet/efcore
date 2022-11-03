// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that gets values from <see cref="ShapedQueryExpression.QueryExpression" /> to be used in
///         <see cref="ShapedQueryExpression.ShaperExpression" /> while creating results.
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
public class ProjectionBindingExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ProjectionBindingExpression" /> class.
    /// </summary>
    /// <param name="queryExpression">The query expression to get the value from.</param>
    /// <param name="projectionMember">The projection member to bind with query expression.</param>
    /// <param name="type">The clr type of value being read.</param>
    public ProjectionBindingExpression(
        Expression queryExpression,
        ProjectionMember projectionMember,
        Type type)
    {
        QueryExpression = queryExpression;
        ProjectionMember = projectionMember;
        Type = type;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ProjectionBindingExpression" /> class.
    /// </summary>
    /// <param name="queryExpression">The query expression to get the value from.</param>
    /// <param name="index">The index to bind with query expression projection.</param>
    /// <param name="type">The clr type of value being read.</param>
    public ProjectionBindingExpression(
        Expression queryExpression,
        int index,
        Type type)
    {
        QueryExpression = queryExpression;
        Index = index;
        Type = type;
    }

    /// <summary>
    ///     The query expression to bind with.
    /// </summary>
    public virtual Expression QueryExpression { get; }

    /// <summary>
    ///     The projection member to bind if binding is via projection member.
    /// </summary>
    public virtual ProjectionMember? ProjectionMember { get; }

    /// <summary>
    ///     The projection member to bind if binding is via projection index.
    /// </summary>
    public virtual int? Index { get; }

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(nameof(ProjectionBindingExpression) + ": ");
        if (ProjectionMember != null)
        {
            expressionPrinter.Append(ProjectionMember.ToString());
        }
        else if (Index != null)
        {
            expressionPrinter.Append(Index.ToString()!);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ProjectionBindingExpression projectionBindingExpression
                && Equals(projectionBindingExpression));

    private bool Equals(ProjectionBindingExpression projectionBindingExpression)
        => QueryExpression.Equals(projectionBindingExpression.QueryExpression)
            && Type == projectionBindingExpression.Type
            && (ProjectionMember?.Equals(projectionBindingExpression.ProjectionMember)
                ?? projectionBindingExpression.ProjectionMember == null)
            && Index == projectionBindingExpression.Index;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(QueryExpression, ProjectionMember, Index);
}
