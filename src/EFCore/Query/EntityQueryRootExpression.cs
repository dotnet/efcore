// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents an entity query root in query expression.
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
public class EntityQueryRootExpression : QueryRootExpression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="EntityQueryRootExpression" /> class with associated query provider.
    /// </summary>
    /// <param name="asyncQueryProvider">The query provider associated with this query root.</param>
    /// <param name="entityType">The entity type this query root represents.</param>
    public EntityQueryRootExpression(IAsyncQueryProvider asyncQueryProvider, IEntityType entityType)
        : base(asyncQueryProvider, entityType.ClrType)
    {
        EntityType = entityType;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="EntityQueryRootExpression" /> class without any query provider.
    /// </summary>
    /// <param name="entityType">The entity type this query root represents.</param>
    public EntityQueryRootExpression(IEntityType entityType)
        : base(entityType.ClrType)
    {
        EntityType = entityType;
    }

    /// <summary>
    ///     The entity type represented by this query root.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     Detaches the associated query provider from this query root expression.
    /// </summary>
    /// <returns>A new query root expression without query provider.</returns>
    public override Expression DetachQueryProvider()
        => new EntityQueryRootExpression(EntityType);

    /// <summary>
    ///     Updates entity type associated with this query root with equivalent optimized version.
    /// </summary>
    /// <param name="entityType">The entity type to replace with.</param>
    /// <returns>New query root containing given entity type.</returns>
    public virtual EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
            || entityType.Name != EntityType.Name
                ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
                : new EntityQueryRootExpression(entityType);

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

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
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(
            EntityType.HasSharedClrType
                ? $"DbSet<{EntityType.ClrType.ShortDisplayName()}>(\"{EntityType.Name}\")"
                : $"DbSet<{EntityType.ClrType.ShortDisplayName()}>()");

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => Print(expressionPrinter);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is EntityQueryRootExpression queryRootExpression
                && EntityType == queryRootExpression.EntityType);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), EntityType);
}
