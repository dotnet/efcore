// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         A service to traverse a graph of entities and perform some action on at each node.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IEntityEntryGraphIterator
    {
        /// <summary>
        ///     Traverses a graph of entities allowing an action to be taken at each node.
        /// </summary>
        /// <param name="node"> The node that is being visited. </param>
        /// <param name="handleNode"> A delegate to call to handle the node. </param>
        /// <typeparam name="TState"> The type of the state object. </typeparam>
        void TraverseGraph<TState>(
            [NotNull] EntityEntryGraphNode<TState> node,
            [NotNull] Func<EntityEntryGraphNode<TState>, bool> handleNode);

        /// <summary>
        ///     Traverses a graph of entities allowing an action to be taken at each node.
        /// </summary>
        /// <param name="node"> The node that is being visited. </param>
        /// <param name="handleNode"> A delegate to call to handle the node. </param>
        /// <param name="cancellationToken">  A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <typeparam name="TState"> The type of the state object. </typeparam>
        /// <returns> A task that represents the asynchronous operation. </returns>
        Task TraverseGraphAsync<TState>(
            [NotNull] EntityEntryGraphNode<TState> node,
            [NotNull] Func<EntityEntryGraphNode<TState>, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default);
    }
}
