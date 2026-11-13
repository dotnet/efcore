// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         The principal data structure used by a compiled query during execution.
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
public abstract class QueryContext
{
    private IStateManager? _stateManager;
    private HashSet<InternalEntityEntry>? _entriesStartedTracking;

    /// <summary>
    ///     <para>
    ///         Creates a new <see cref="QueryContext" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    protected QueryContext(QueryContextDependencies dependencies)
    {
        Dependencies = dependencies;
        Context = dependencies.CurrentContext.Context;
    }

    /// <summary>
    ///     The current <see cref="DbContext" /> in using while executing the query.
    /// </summary>
    public virtual DbContext Context { get; }

    /// <summary>
    ///     The query parameter used in the query.
    /// </summary>
    public virtual Dictionary<string, object?> Parameters { get; } = [];

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryContextDependencies Dependencies { get; }

    /// <summary>
    ///     Sets the navigation for given entity as loaded.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="navigation">The navigation property.</param>
    public virtual void SetNavigationIsLoaded(object entity, INavigationBase navigation)
        // InitializeStateManager will populate the field before calling here
        => _stateManager!.TryGetEntry(entity)!.SetIsLoaded(navigation);

    /// <summary>
    ///     The execution strategy to use while executing the query.
    /// </summary>
    public virtual IExecutionStrategy ExecutionStrategy
        => Dependencies.ExecutionStrategy;

    /// <summary>
    ///     The concurrency detector to use while executing the query.
    /// </summary>
    public virtual IConcurrencyDetector ConcurrencyDetector
        => Dependencies.ConcurrencyDetector;

    /// <summary>
    ///     The exception detector to use while executing the query.
    /// </summary>
    public virtual IExceptionDetector ExceptionDetector
        => Dependencies.ExceptionDetector;

    /// <summary>
    ///     The cancellation token to use while executing the query.
    /// </summary>
    public virtual CancellationToken CancellationToken { get; set; }

    /// <summary>
    ///     The command logger to use while executing the query.
    /// </summary>
    public virtual IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger
        => Dependencies.CommandLogger;

    /// <summary>
    ///     The query logger to use while executing the query.
    /// </summary>
    public virtual IDiagnosticsLogger<DbLoggerCategory.Query> QueryLogger
        => Dependencies.QueryLogger;

    /// <summary>
    ///     Initializes the <see cref="IStateManager" /> to be used with this QueryContext.
    /// </summary>
    /// <param name="standAlone">Whether a stand-alone <see cref="IStateManager" /> should be created to perform identity resolution.</param>
    public virtual void InitializeStateManager(bool standAlone = false)
    {
        _stateManager ??= standAlone
            ? new StateManager(Dependencies.StateManager.Dependencies)
            : Dependencies.StateManager;

        // This is called each time the query is executed. Entities that were started being tracked by an earlier execution
        // are tracked by then, and can have unsaved changes.
        _entriesStartedTracking?.Clear();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InternalEntityEntry? TryGetEntry(
        IKey key,
        object[] keyValues,
        bool throwOnNullKey,
        out bool hasNullKey)
    {
        // InitializeStateManager will populate the field before calling here
        var entry = _stateManager!.TryGetEntry(key, keyValues, throwOnNullKey, out hasNullKey);

        // An entity returned by a tracking query provably exists in the store, so an entry that is being tracked as
        // Added (e.g. a new entity whose key matches an existing row, possibly created during an Add graph traversal)
        // must be corrected to Unchanged. Otherwise it would be re-inserted, causing a duplicate key. See #35762.
        if (entry is { EntityState: EntityState.Added })
        {
            entry.SetEntityState(EntityState.Unchanged);
        }

        return entry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InternalEntityEntry StartTracking(
            IEntityType entityType,
            object entity,
            in ISnapshot snapshot)
    {
        // InitializeStateManager will populate the field before calling here
        var entry = _stateManager!.StartTrackingFromQuery(entityType, entity, snapshot);

        // Only entities that distinguish between null and empty collections need to know later on if they were already tracked
        if (entityType is IRuntimeEntityType { NullableCollectionCount: > 0 })
        {
            (_entriesStartedTracking ??= []).Add(entry);
        }

        return entry;
    }

    /// <summary>
    ///     Gets a value indicating whether the stored null-ness of a collection navigation that has
    ///     <see cref="IReadOnlyNavigation.IsNullable" /> equal to <see langword="true" /> should be applied to the given entity instance.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is <see langword="true" /> for an entity that is not tracked, for example in a no-tracking query, and for an entity
    ///         that was started being tracked by this query. It is <see langword="false" /> for an entity that was already tracked
    ///         before this query, in which case the current values of the entity are kept, including an unsaved change between a null
    ///         and a non-null collection.
    ///     </para>
    ///     <para>
    ///         A database provider should call this before materializing the collection and, if it returns
    ///         <see langword="false" />, not modify the collection or call <see cref="SetCollectionNullnessLoaded" />.
    ///     </para>
    /// </remarks>
    /// <param name="navigation">The collection navigation.</param>
    /// <param name="entity">The entity instance that declares the navigation.</param>
    /// <returns><see langword="true" /> if the stored null-ness should be applied, otherwise <see langword="false" />.</returns>
    public virtual bool CanLoadCollectionNullness(INavigation navigation, object entity)
        => IsNullnessLoadable(TryGetEntry(entity));

    private bool IsNullnessLoadable(InternalEntityEntry? entry)
        => entry == null
            || _entriesStartedTracking?.Contains(entry) == true;

    /// <summary>
    ///     Sets the loaded null-ness of a collection navigation that has <see cref="IReadOnlyNavigation.IsNullable" /> equal to
    ///     <see langword="true" /> on the given entity instance, and records it as the value that was read from the database so that a
    ///     later change between a null and a non-null collection is detected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If <paramref name="isNull" /> is <see langword="true" />, the navigation is set to <see langword="null" />, without
    ///         using collection initializers or raising change notifications. Otherwise, the collection must already have been created
    ///         and populated by the caller.
    ///     </para>
    ///     <para>
    ///         This does nothing if <see cref="CanLoadCollectionNullness" /> returns <see langword="false" />.
    ///     </para>
    /// </remarks>
    /// <param name="navigation">The collection navigation.</param>
    /// <param name="entity">The entity instance that declares the navigation.</param>
    /// <param name="isNull">Whether the collection was stored as <see langword="null" />.</param>
    public virtual void SetCollectionNullnessLoaded(INavigation navigation, object entity, bool isNull)
    {
        var entry = TryGetEntry(entity);
        if (!IsNullnessLoadable(entry))
        {
            return;
        }

        var recordOriginal = entry != null && navigation.GetOriginalValueIndex() >= 0;
        if (isNull)
        {
            // The original value is recorded first so that a setter raising notifications isn't seen as a change.
            if (recordOriginal)
            {
                entry!.SetOriginalValue(navigation, null, skipChangeDetection: true);
            }

            ((IRuntimePropertyBase)navigation).MaterializationSetter.SetClrValue(entity, null);
        }

        // A setter can have replaced null, in which case that is the value that has been loaded.
        if (recordOriginal)
        {
            var currentValue = navigation.GetGetter().GetClrValue(entity);
            if (!isNull
                || currentValue != null)
            {
                entry!.SetOriginalValue(navigation, currentValue, skipChangeDetection: true);
            }
        }

        // Loading the collection must not leave the navigation looking modified because of a change notification that was
        // raised by the entity while the collection was set. The navigation was known not to be modified before it was loaded.
        if (entry != null
            && entry.IsModified(navigation))
        {
            entry.SetPropertyModified(navigation, false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InternalEntityEntry? TryGetEntry(object entity)
        // InitializeStateManager will populate the field before calling here
        => _stateManager!.TryGetEntry(entity, throwOnNonUniqueness: false);
}
