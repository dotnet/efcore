// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityGraphAttacher : IEntityGraphAttacher
{
    private readonly IEntityEntryGraphIterator _graphIterator;
    private HashSet<object>? _visited;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityGraphAttacher(
        IEntityEntryGraphIterator graphIterator)
    {
        _graphIterator = graphIterator;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AttachGraph(
        InternalEntityEntry rootEntry,
        EntityState targetState,
        EntityState storeGeneratedWithKeySetTargetState,
        bool forceStateWhenUnknownKey)
    {
        try
        {
            rootEntry.StateManager.BeginAttachGraph();

            _graphIterator.TraverseGraph(
                new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                    rootEntry,
                    (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                    null,
                    null),
                PaintAction);

            _visited = null;
            rootEntry.StateManager.CompleteAttachGraph();
        }
        catch
        {
            _visited = null;
            rootEntry.StateManager.AbortAttachGraph();
            throw;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task AttachGraphAsync(
        InternalEntityEntry rootEntry,
        EntityState targetState,
        EntityState storeGeneratedWithKeySetTargetState,
        bool forceStateWhenUnknownKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            rootEntry.StateManager.BeginAttachGraph();

            await _graphIterator.TraverseGraphAsync(
                new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                    rootEntry,
                    (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                    null,
                    null),
                PaintActionAsync,
                cancellationToken).ConfigureAwait(false);

            _visited = null;
            rootEntry.StateManager.CompleteAttachGraph();
        }
        catch
        {
            _visited = null;
            rootEntry.StateManager.AbortAttachGraph();
            throw;
        }
    }

    private bool PaintAction(
        EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node)
    {
        SetReferenceLoaded(node);

        var internalEntityEntry = node.GetInfrastructure();
        if (internalEntityEntry.EntityState != EntityState.Detached
            || (_visited != null && _visited.Contains(internalEntityEntry.Entity)))
        {
            return false;
        }

        var (targetState, storeGenTargetState, force) = node.NodeState;

        var (isGenerated, isSet) = internalEntityEntry.IsKeySet;

        if (internalEntityEntry.StateManager.ResolveToExistingEntry(
                internalEntityEntry,
                node.InboundNavigation, node.SourceEntry?.GetInfrastructure()))
        {
            (_visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance)).Add(internalEntityEntry.Entity);
        }
        else
        {
            internalEntityEntry.SetEntityState(
                isSet
                    ? (isGenerated ? storeGenTargetState : targetState)
                    : EntityState.Added, // Key can only be not-set if it is store-generated
                acceptChanges: true,
                forceStateWhenUnknownKey: force ? targetState : null,
                fallbackState: targetState);
        }

        return true;
    }

    private async Task<bool> PaintActionAsync(
        EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node,
        CancellationToken cancellationToken)
    {
        SetReferenceLoaded(node);

        var internalEntityEntry = node.GetInfrastructure();
        if (internalEntityEntry.EntityState != EntityState.Detached
            || (_visited != null && _visited.Contains(internalEntityEntry.Entity)))
        {
            return false;
        }

        var (targetState, storeGenTargetState, force) = node.NodeState;

        var (isGenerated, isSet) = internalEntityEntry.IsKeySet;

        if (internalEntityEntry.StateManager.ResolveToExistingEntry(
                internalEntityEntry,
                node.InboundNavigation, node.SourceEntry?.GetInfrastructure()))
        {
            (_visited ??= []).Add(internalEntityEntry.Entity);
        }
        else
        {
            await internalEntityEntry.SetEntityStateAsync(
                    isSet
                        ? (isGenerated ? storeGenTargetState : targetState)
                        : EntityState.Added, // Key can only be not-set if it is store-generated
                    acceptChanges: true,
                    forceStateWhenUnknownKey: force ? targetState : null,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    private static void SetReferenceLoaded(
        EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node)
    {
        var inboundNavigation = node.InboundNavigation;
        if (inboundNavigation is { IsCollection: false })
        {
            node.SourceEntry!.GetInfrastructure().SetIsLoaded(inboundNavigation);
        }
    }
}
