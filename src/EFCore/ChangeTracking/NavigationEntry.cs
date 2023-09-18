// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking and loading information for a navigation property
///     that associates this entity to one or more other entities.
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
public abstract class NavigationEntry : MemberEntry
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected NavigationEntry(InternalEntityEntry internalEntry, string name, bool collection)
        : this(internalEntry, GetNavigation(internalEntry, name), collection)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected NavigationEntry(InternalEntityEntry internalEntry, INavigationBase navigationBase, bool collection)
        : base(internalEntry, navigationBase)
    {
        if (collection
            && !navigationBase.IsCollection)
        {
            throw new InvalidOperationException(
                CoreStrings.CollectionIsReference(
                    navigationBase.Name, internalEntry.EntityType.DisplayName(),
                    nameof(ChangeTracking.EntityEntry.Collection), nameof(ChangeTracking.EntityEntry.Reference)));
        }

        if (!collection
            && navigationBase.IsCollection)
        {
            throw new InvalidOperationException(
                CoreStrings.ReferenceIsCollection(
                    navigationBase.Name, internalEntry.EntityType.DisplayName(),
                    nameof(ChangeTracking.EntityEntry.Reference), nameof(ChangeTracking.EntityEntry.Collection)));
        }
    }

    private static INavigationBase GetNavigation(InternalEntityEntry internalEntry, string name)
    {
        var navigation = (INavigationBase?)internalEntry.EntityType.FindNavigation(name)
            ?? internalEntry.EntityType.FindSkipNavigation(name);

        if (navigation == null)
        {
            if (internalEntry.EntityType.FindProperty(name) != null
                || internalEntry.EntityType.FindComplexProperty(name) != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationIsProperty(
                        name, internalEntry.EntityType.DisplayName(),
                        nameof(ChangeTracking.EntityEntry.Reference), nameof(ChangeTracking.EntityEntry.Collection),
                        nameof(ChangeTracking.EntityEntry.Property)));
            }

            throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, internalEntry.EntityType.DisplayName()));
        }

        return navigation;
    }

    /// <summary>
    ///     Loads the entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public abstract void Load();

    /// <summary>
    ///     Loads the entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="options">Options to control the way related entities are loaded.</param>
    public abstract void Load(LoadOptions options);

    /// <summary>
    ///     Loads entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true" />.
    /// </summary>
    /// <remarks>
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
    public abstract Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Loads entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
    ///     is already set to <see langword="true" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use <see langword="await" /> to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="options">Options to control the way related entities are loaded.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public abstract Task LoadAsync(LoadOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the query that would be used by <see cref="Load()" /> to load entities referenced by
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
    public abstract IQueryable Query();

    /// <summary>
    ///     Gets or sets a value indicating whether the entity or entities referenced by this navigation property
    ///     are known to be loaded.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Loading entities from the database using
    ///         <see cref="EntityFrameworkQueryableExtensions.Include{TEntity,TProperty}" /> or
    ///         <see
    ///             cref="EntityFrameworkQueryableExtensions.ThenInclude{TEntity,TPreviousProperty,TProperty}(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable{TEntity,System.Collections.Generic.IEnumerable{TPreviousProperty}},System.Linq.Expressions.Expression{System.Func{TPreviousProperty,TProperty}})" />
    ///         , <see cref="Load()" />, or <see cref="LoadAsync(CancellationToken)" /> will set this flag. Subsequent calls to
    ///         <see cref="Load()" />
    ///         or <see cref="LoadAsync(CancellationToken)" /> will then be a no-op.
    ///     </para>
    ///     <para>
    ///         It is possible for IsLoaded to be false even if all related entities are loaded. This is because, depending on
    ///         how entities are loaded, it is not always possible to know for sure that all entities in a related collection
    ///         have been loaded. In such cases, calling <see cref="Load()" /> or <see cref="LoadAsync(CancellationToken)" /> will ensure all
    ///         related entities are loaded and will set this flag to <see langword="true" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <value>
    ///     <see langword="true" /> if all the related entities are loaded or the IsLoaded has been explicitly set to <see langword="true" />.
    /// </value>
    public virtual bool IsLoaded
    {
        get => InternalEntry.IsLoaded(Metadata);
        set => InternalEntry.SetIsLoaded(Metadata, value);
    }

    /// <summary>
    ///     Gets the metadata that describes the facets of this property and how it maps to the database.
    /// </summary>
    public new virtual INavigationBase Metadata
        => (INavigationBase)base.Metadata;
}
