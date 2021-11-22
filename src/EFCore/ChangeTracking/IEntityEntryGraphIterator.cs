// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     A service to traverse a graph of entities and perform some action on at each node.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-track-graph">Tracking entities in EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public interface IEntityEntryGraphIterator
{
    /// <summary>
    ///     Traverses a graph of entities allowing an action to be taken at each node.
    /// </summary>
    /// <param name="node">The node that is being visited.</param>
    /// <param name="handleNode">A delegate to call to handle the node.</param>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    void TraverseGraph<TState>(
        EntityEntryGraphNode<TState> node,
        Func<EntityEntryGraphNode<TState>, bool> handleNode);

    /// <summary>
    ///     Traverses a graph of entities allowing an action to be taken at each node.
    /// </summary>
    /// <param name="node">The node that is being visited.</param>
    /// <param name="handleNode">A delegate to call to handle the node.</param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task TraverseGraphAsync<TState>(
        EntityEntryGraphNode<TState> node,
        Func<EntityEntryGraphNode<TState>, CancellationToken, Task<bool>> handleNode,
        CancellationToken cancellationToken = default);
}
