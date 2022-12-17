// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
///     navigation property that associates this entity to another entity.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
///         not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
///     </para>
/// </remarks>
public class ReferenceEntry : NavigationEntry
{
    private IEntityFinder? _finder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceEntry(InternalEntityEntry internalEntry, string name)
        : base(internalEntry, name, collection: false)
    {
        LocalDetectChanges();

        // ReSharper disable once VirtualMemberCallInConstructor
        Check.DebugAssert(Metadata is INavigation, "Issue #21673. Non-collection skip navigations not supported.");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceEntry(InternalEntityEntry internalEntry, INavigation navigation)
        : base(internalEntry, navigation, collection: false)
    {
        LocalDetectChanges();

        // ReSharper disable once VirtualMemberCallInConstructor
        Check.DebugAssert(Metadata is INavigation, "Issue #21673. Non-collection skip navigations not supported.");
    }

    private void LocalDetectChanges()
    {
        if (!(Metadata is INavigation navigation
                && navigation.IsOnDependent))
        {
            var target = GetTargetEntry();
            if (target != null)
            {
                var context = InternalEntry.Context;
                if (context.ChangeTracker.AutoDetectChangesEnabled
                    && !((IRuntimeModel)context.Model).SkipDetectChanges)
                {
                    context.GetDependencies().ChangeDetector.DetectChanges(target);
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
        if (!IsLoaded)
        {
            TargetFinder.Load((INavigation)Metadata, InternalEntry, forceIdentityResolution: false);
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
        if (!IsLoaded)
        {
            TargetFinder.Load((INavigation)Metadata, InternalEntry, forceIdentityResolution: true);
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
        => IsLoaded
            ? Task.CompletedTask
            : TargetFinder.LoadAsync((INavigation)Metadata, InternalEntry, forceIdentityResolution: false, cancellationToken);

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
        => IsLoaded
            ? Task.CompletedTask
            : TargetFinder.LoadAsync((INavigation)Metadata, InternalEntry, forceIdentityResolution: true, cancellationToken);

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
    /// <returns>The query to load related entities.</returns>
    public override IQueryable Query()
        => TargetFinder.Query((INavigation)Metadata, InternalEntry);

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
            var navigation = (INavigation)Metadata;

            return navigation.IsOnDependent
                ? navigation.ForeignKey.Properties.Any(InternalEntry.IsModified)
                : AnyFkPropertiesModified(navigation, CurrentValue);
        }
        set
        {
            var navigation = (INavigation)Metadata;

            if (navigation.IsOnDependent)
            {
                SetFkPropertiesModified(navigation, InternalEntry, value);
            }
            else
            {
                var navigationValue = CurrentValue;
                if (navigationValue != null)
                {
                    var relatedEntry = InternalEntry.StateManager.TryGetEntry(navigationValue, Metadata.TargetEntityType);
                    if (relatedEntry != null)
                    {
                        SetFkPropertiesModified(navigation, relatedEntry, value);
                    }
                }
            }
        }
    }

    private static void SetFkPropertiesModified(
        INavigation navigation,
        InternalEntityEntry internalEntityEntry,
        bool modified)
    {
        var anyNonPk = navigation.ForeignKey.Properties.Any(p => !p.IsPrimaryKey());
        foreach (var property in navigation.ForeignKey.Properties)
        {
            if (anyNonPk
                && !property.IsPrimaryKey())
            {
                internalEntityEntry.SetPropertyModified(property, isModified: modified, acceptChanges: false);
            }
        }
    }

    private bool AnyFkPropertiesModified(INavigation navigation, object? relatedEntity)
    {
        if (relatedEntity == null)
        {
            return false;
        }

        var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity, Metadata.TargetEntityType);

        return relatedEntry != null
            && (relatedEntry.EntityState == EntityState.Added
                || relatedEntry.EntityState == EntityState.Deleted
                || navigation.ForeignKey.Properties.Any(relatedEntry.IsModified));
    }

    /// <summary>
    ///     The <see cref="EntityEntry" /> of the entity this navigation targets.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <value> An entry for the entity that this navigation targets. </value>
    public virtual EntityEntry? TargetEntry
    {
        get
        {
            var target = GetTargetEntry();
            return target == null ? null : new EntityEntry(target);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalEntityEntry? GetTargetEntry()
        => CurrentValue == null
            ? null
            : InternalEntry.StateManager.GetOrCreateEntry(CurrentValue, Metadata.TargetEntityType);

    private IEntityFinder TargetFinder
        => _finder ??= InternalEntry.StateManager.CreateEntityFinder(Metadata.TargetEntityType);
}
