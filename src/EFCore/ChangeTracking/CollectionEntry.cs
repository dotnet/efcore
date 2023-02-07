// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking and loading information for a collection
///     navigation property that associates this entity to a collection of another entities.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
///         not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>,
///         <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>,
///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
///     </para>
/// </remarks>
public class CollectionEntry : NavigationEntry
{
    private ICollectionLoader? _loader;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public CollectionEntry(InternalEntityEntry internalEntry, string name)
        : base(internalEntry, name, collection: true)
    {
        LocalDetectChanges();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public CollectionEntry(InternalEntityEntry internalEntry, INavigationBase navigationBase)
        : base(internalEntry, navigationBase, collection: true)
    {
        LocalDetectChanges();
    }

    private void LocalDetectChanges()
    {
        if (Metadata.IsShadowProperty())
        {
            EnsureInitialized();
        }

        var collection = CurrentValue;
        if (collection != null)
        {
            var targetType = Metadata.TargetEntityType;
            var context = InternalEntry.Context;

            var changeDetector = context.ChangeTracker.AutoDetectChangesEnabled
                && !((IRuntimeModel)context.Model).SkipDetectChanges
                    ? context.GetDependencies().ChangeDetector
                    : null;

            foreach (var entity in collection.OfType<object>().ToList())
            {
                var entry = InternalEntry.StateManager.GetOrCreateEntry(entity, targetType);
                changeDetector?.DetectChanges(entry);
            }
        }
    }

    /// <summary>
    ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
    ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
    ///     for the context to detect the change.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    public new virtual IEnumerable? CurrentValue
    {
        get => (IEnumerable?)base.CurrentValue;
        set => base.CurrentValue = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether any of foreign key property values associated
    ///     with this navigation property have been modified and should be updated in the database
    ///     when <see cref="DbContext.SaveChanges()" /> is called.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    public override bool IsModified
    {
        get
        {
            var stateManager = InternalEntry.StateManager;

            if (Metadata is ISkipNavigation skipNavigation)
            {
                if (InternalEntry.EntityState != EntityState.Unchanged
                    && InternalEntry.EntityState != EntityState.Detached)
                {
                    return true;
                }

                var joinEntityType = skipNavigation.JoinEntityType;
                var foreignKey = skipNavigation.ForeignKey;
                var inverseForeignKey = skipNavigation.Inverse.ForeignKey;
                foreach (var joinEntry in stateManager.Entries)
                {
                    if (joinEntry.EntityType == joinEntityType
                        && stateManager.FindPrincipal(joinEntry, foreignKey) == InternalEntry
                        && (joinEntry.EntityState == EntityState.Added
                            || joinEntry.EntityState == EntityState.Deleted
                            || foreignKey.Properties.Any(joinEntry.IsModified)
                            || inverseForeignKey.Properties.Any(joinEntry.IsModified)
                            || (stateManager.FindPrincipal(joinEntry, inverseForeignKey)?.EntityState == EntityState.Deleted)))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var navigationValue = CurrentValue;
                if (navigationValue != null)
                {
                    var targetEntityType = Metadata.TargetEntityType;
                    var foreignKey = ((INavigation)Metadata).ForeignKey;

                    foreach (var relatedEntity in navigationValue)
                    {
                        var relatedEntry = stateManager.TryGetEntry(relatedEntity, targetEntityType);

                        if (relatedEntry != null
                            && (relatedEntry.EntityState == EntityState.Added
                                || relatedEntry.EntityState == EntityState.Deleted
                                || foreignKey.Properties.Any(relatedEntry.IsModified)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        set
        {
            var stateManager = InternalEntry.StateManager;

            if (Metadata is ISkipNavigation skipNavigation)
            {
                var joinEntityType = skipNavigation.JoinEntityType;
                var foreignKey = skipNavigation.ForeignKey;
                foreach (var joinEntry in stateManager
                             .GetEntriesForState(added: !value, modified: !value, deleted: !value, unchanged: value).Where(
                                 e => e.EntityType == joinEntityType
                                     && stateManager.FindPrincipal(e, foreignKey) == InternalEntry)
                             .ToList())
                {
                    joinEntry.SetEntityState(value ? EntityState.Modified : EntityState.Unchanged);
                }
            }
            else
            {
                var foreignKey = ((INavigation)Metadata).ForeignKey;
                var navigationValue = CurrentValue;
                if (navigationValue != null)
                {
                    foreach (var relatedEntity in navigationValue)
                    {
                        var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity, Metadata.TargetEntityType);
                        if (relatedEntry != null)
                        {
                            var anyNonPk = foreignKey.Properties.Any(p => !p.IsPrimaryKey());
                            foreach (var property in foreignKey.Properties)
                            {
                                if (anyNonPk
                                    && !property.IsPrimaryKey())
                                {
                                    relatedEntry.SetPropertyModified(property, isModified: value, acceptChanges: false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Loads the entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the the entity represented by this entry is tracked, then entities with the same primary key value are not replaced
    ///         by new entities or overwritten with new data from the database. If the entity entity represented by this entry is not
    ///         tracked and the collection already contains entities, then calling this method will result in duplicate
    ///         instances in the collection or inverse collection for any entities with the same key value.
    ///         Use <see cref="LoadWithIdentityResolution" /> to avoid getting these duplicates.
    ///     </para>
    ///     <para>
    ///         For tracked entities, this method behaves in the same way and has the same performance as
    ///         <see cref="LoadWithIdentityResolution" />. For entities that are not tracked, this method can be faster than
    ///         <see cref="LoadWithIdentityResolution" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public override void Load()
    {
        EnsureInitialized();

        if (!IsLoaded)
        {
            TargetLoader.Load(InternalEntry, forceIdentityResolution: false);
        }
    }

    /// <summary>
    ///     Loads the entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entities with the same primary key value are not replaced by new entities or overwritten with new data from the database.
    ///         This navigation and its inverse will not contain duplicate entities.
    ///     </para>
    ///     <para>
    ///         For tracked entities, this method behaves in the same way and has the same performance as
    ///         <see cref="Load" />. For entities that are not tracked, this method can be slower than <see cref="Load" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public override void LoadWithIdentityResolution()
    {
        EnsureInitialized();

        if (!IsLoaded)
        {
            TargetLoader.Load(InternalEntry, forceIdentityResolution: true);
        }
    }

    /// <summary>
    ///     Loads entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the the entity represented by this entry is tracked, then entities with the same primary key value are not replaced
    ///         by new entities or overwritten with new data from the database. If the entity entity represented by this entry is not
    ///         tracked and the collection already contains entities, then calling this method will result in duplicate
    ///         instances in the collection or inverse collection for any entities with the same key value.
    ///         Use <see cref="LoadWithIdentityResolutionAsync" /> to avoid getting these duplicates.
    ///     </para>
    ///     <para>
    ///         For tracked entities, this method behaves in the same way and has the same performance as
    ///         <see cref="LoadWithIdentityResolutionAsync" />. For entities that are not tracked, this method can be faster than
    ///         <see cref="LoadWithIdentityResolutionAsync" />.
    ///     </para>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        return IsLoaded
            ? Task.CompletedTask
            : TargetLoader.LoadAsync(InternalEntry, forceIdentityResolution: false, cancellationToken);
    }

    /// <summary>
    ///     Loads entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entities with the same primary key value are not replaced by new entities or overwritten with new data from the database.
    ///         This navigation and its inverse will not contain duplicate entities.
    ///     </para>
    ///     <para>
    ///         For tracked entities, this method behaves in the same way and has the same performance as
    ///         <see cref="LoadAsync" />. For entities that are not tracked, this method can be slower than <see cref="LoadAsync" />.
    ///     </para>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public override Task LoadWithIdentityResolutionAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        return IsLoaded
            ? Task.CompletedTask
            : TargetLoader.LoadAsync(InternalEntry, forceIdentityResolution: true, cancellationToken);
    }

    /// <summary>
    ///     Returns the query that would be used by <see cref="Load" /> to load entities referenced by
    ///     this navigation property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The query can be composed over using LINQ to perform filtering, counting, etc. without
    ///         actually loading all entities from the database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public override IQueryable Query()
    {
        EnsureInitialized();

        return TargetLoader.Query(InternalEntry);
    }

    private void EnsureInitialized()
        => InternalEntry.GetOrCreateCollection(Metadata, forMaterialization: true);

    /// <summary>
    ///     The <see cref="EntityEntry" /> of an entity this navigation targets.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="entity">The entity to get the entry for.</param>
    /// <value> An entry for an entity that this navigation targets. </value>
    public virtual EntityEntry? FindEntry(object entity)
    {
        var entry = GetInternalTargetEntry(entity);
        return entry == null
            ? null
            : new EntityEntry(entry);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalEntityEntry? GetInternalTargetEntry(object entity)
        => CurrentValue == null
            || !InternalEntry.CollectionContains(Metadata, entity)
                ? null
                : InternalEntry.StateManager.GetOrCreateEntry(entity, Metadata.TargetEntityType);

    private ICollectionLoader TargetLoader
        => _loader ??= Metadata is IRuntimeSkipNavigation skipNavigation
            ? skipNavigation.GetManyToManyLoader()
            : new EntityFinderCollectionLoaderAdapter(
                InternalEntry.StateManager.CreateEntityFinder(Metadata.TargetEntityType),
                (INavigation)Metadata);
}
