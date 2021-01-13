// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class EntityEntryGraphIterator : IEntityEntryGraphIterator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void TraverseGraph<TState>(
            EntityEntryGraphNode<TState> node,
            Func<EntityEntryGraphNode<TState>, bool> handleNode)
        {
            if (!handleNode(node))
            {
                return;
            }

            var internalEntityEntry = node.GetInfrastructure();
            var navigations = internalEntityEntry.EntityType.GetNavigations()
                .Concat<INavigationBase>(internalEntityEntry.EntityType.GetSkipNavigations());

            var stateManager = internalEntityEntry.StateManager;

            foreach (var navigation in navigations)
            {
                var navigationValue = internalEntityEntry[navigation];

                if (navigationValue != null)
                {
                    var targetEntityType = navigation.TargetEntityType;
                    if (navigation.IsCollection)
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            var targetEntry = stateManager.GetOrCreateEntry(relatedEntity, targetEntityType);
                            TraverseGraph(
                                (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                                handleNode);
                        }
                    }
                    else
                    {
                        var targetEntry = stateManager.GetOrCreateEntry(navigationValue, targetEntityType);
                        TraverseGraph(
                            (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                            handleNode);
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task TraverseGraphAsync<TState>(
            EntityEntryGraphNode<TState> node,
            Func<EntityEntryGraphNode<TState>, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default)
        {
            if (!await handleNode(node, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            var internalEntityEntry = node.GetInfrastructure();
            var navigations = internalEntityEntry.EntityType.GetNavigations()
                .Concat<INavigationBase>(internalEntityEntry.EntityType.GetSkipNavigations());
            var stateManager = internalEntityEntry.StateManager;

            foreach (var navigation in navigations)
            {
                var navigationValue = internalEntityEntry[navigation];

                if (navigationValue != null)
                {
                    var targetType = navigation.TargetEntityType;
                    if (navigation.IsCollection)
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            var targetEntry = stateManager.GetOrCreateEntry(relatedEntity, targetType);
                            await TraverseGraphAsync(
                                    (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                                    handleNode,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var targetEntry = stateManager.GetOrCreateEntry(navigationValue, targetType);
                        await TraverseGraphAsync(
                                (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                                handleNode,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
