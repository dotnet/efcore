// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ManyToManyLoader<TEntity, TSourceEntity> : ICollectionLoader<TEntity>
    where TEntity : class
    where TSourceEntity : class
{
    private readonly ISkipNavigation _skipNavigation;
    private readonly ISkipNavigation _inverseSkipNavigation;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ManyToManyLoader(ISkipNavigation skipNavigation)
    {
        _skipNavigation = skipNavigation;
        _inverseSkipNavigation = skipNavigation.Inverse;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Load(InternalEntityEntry entry, LoadOptions options)
    {
        var keyValues = PrepareForLoad(entry);

        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            var queryable = Query(entry.Context, keyValues, entry, options);

            if (entry.EntityState == EntityState.Detached)
            {
                var stateManager = GetOrCreateStateManagerAndStartTrackingIfNeeded(entry, options);
                try
                {
                    foreach (var loaded in queryable)
                    {
                        Fixup(stateManager, entry.Entity, options, loaded);
                    }
                }
                finally
                {
                    if (stateManager != entry.StateManager)
                    {
                        stateManager.Clear(resetting: false);
                    }
                }
            }
            else
            {
                queryable.Load();
            }
        }

        entry.SetIsLoaded(_skipNavigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task LoadAsync(
        InternalEntityEntry entry,
        LoadOptions options,
        CancellationToken cancellationToken = default)
    {
        var keyValues = PrepareForLoad(entry);

        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            var queryable = Query(entry.Context, keyValues, entry, options);

            if (entry.EntityState == EntityState.Detached)
            {
                var stateManager = GetOrCreateStateManagerAndStartTrackingIfNeeded(entry, options);
                try
                {
                    await foreach (var loaded in queryable.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false))
                    {
                        Fixup(stateManager, entry.Entity, options, loaded);
                    }
                }
                finally
                {
                    if (stateManager != entry.StateManager)
                    {
                        stateManager.Clear(resetting: false);
                    }
                }
            }
            else
            {
                await queryable.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        entry.SetIsLoaded(_skipNavigation);
    }

    private void Fixup(IStateManager stateManager, object entity, LoadOptions options, object loaded)
    {
        var entry = stateManager.GetOrCreateEntry(entity);
        var relatedEntry = stateManager.GetOrCreateEntry(loaded);

        if ((options & LoadOptions.ForceIdentityResolution) != 0
            && TryGetTracked(out var existing))
        {
            entry.AddToCollection(_skipNavigation, existing!, forMaterialization: true);
        }
        else
        {
            entry.AddToCollection(_skipNavigation, loaded, forMaterialization: true);
            relatedEntry.AddToCollection(_inverseSkipNavigation, entity, forMaterialization: true);
        }

        bool TryGetTracked(out object? tracked)
        {
            var key = relatedEntry.EntityType.FindPrimaryKey();
            if (key == null)
            {
                tracked = null;
                return false;
            }

            tracked = stateManager.TryGetExistingEntry(relatedEntry.Entity, key)?.Entity;
            return tracked != null;
        }
    }

    private IStateManager GetOrCreateStateManagerAndStartTrackingIfNeeded(InternalEntityEntry entry, LoadOptions options)
    {
        if ((options & LoadOptions.ForceIdentityResolution) == 0)
        {
            return entry.StateManager;
        }

        var stateManager = new StateManager(entry.StateManager.Dependencies);
        StartTracking(stateManager, entry);
        return stateManager;
    }

    private void StartTracking(StateManager stateManager, InternalEntityEntry entry)
    {
        Track(entry.Entity);

        var navigationValue = entry[_skipNavigation];
        if (navigationValue != null)
        {
            foreach (var related in (IEnumerable)navigationValue)
            {
                Track(related);
            }
        }

        void Track(object entity)
            => stateManager.StartTracking(stateManager.GetOrCreateEntry(entity)).MarkUnchangedFromQuery();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IQueryable<TEntity> Query(InternalEntityEntry entry)
    {
        var keyValues = PrepareForLoad(entry);
        var context = entry.Context;

        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues == null)
        {
            // Creates an empty Queryable that works with Async. Has to be an EF query because it could be used in a composition.
            var queryRoot = _skipNavigation.TargetEntityType.HasSharedClrType
                ? context.Set<TEntity>(_skipNavigation.TargetEntityType.Name)
                : context.Set<TEntity>();

            return queryRoot.Where(e => false);
        }

        return Query(context, keyValues, entry, LoadOptions.None);
    }

    private object[]? PrepareForLoad(InternalEntityEntry entry)
    {
        var properties = _skipNavigation.ForeignKey.PrincipalKey.Properties;
        var values = new object[properties.Count];
        var detached = entry.EntityState == EntityState.Detached;

        for (var i = 0; i < values.Length; i++)
        {
            var property = properties[i];
            if (property.IsShadowProperty() && (detached || entry.IsUnknown(property)))
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotLoadDetachedShadow(_skipNavigation.Name, entry.EntityType.DisplayName()));
            }

            var value = entry[properties[i]];
            if (value == null)
            {
                return null;
            }

            values[i] = value;
        }

        return values;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IQueryable ICollectionLoader.Query(InternalEntityEntry entry)
        => Query(entry);

    private IQueryable<TEntity> Query(DbContext context, object[] keyValues, InternalEntityEntry entry, LoadOptions options)
    {
        var loadProperties = _skipNavigation.ForeignKey.PrincipalKey.Properties;

        // Example of query being built:
        //
        // IQueryable<EntityTwo> loaded
        //     = context.Set<EntityOne>()
        //        .AsTracking()
        //        .Where(e => e.Id == left.Id)
        //        .SelectMany(e => e.TwoSkip)
        //        .NotQuiteInclude(e => e.OneSkip.Where(e => e.Id == left.Id));

        var queryRoot = _skipNavigation.DeclaringEntityType.HasSharedClrType
            ? context.Set<TSourceEntity>(_skipNavigation.DeclaringEntityType.Name)
            : context.Set<TSourceEntity>();

        var queryable = queryRoot
            .Where(BuildWhereLambda(loadProperties, new ValueBuffer(keyValues)))
            .SelectMany(BuildSelectManyLambda(_skipNavigation))
            .NotQuiteInclude(BuildIncludeLambda(_skipNavigation.Inverse, loadProperties, new ValueBuffer(keyValues)));

        return entry.EntityState == EntityState.Detached
            ? (options & LoadOptions.ForceIdentityResolution) != 0
                ? queryable.AsNoTrackingWithIdentityResolution()
                : queryable.AsNoTracking()
            : queryable.AsTracking();
    }

    private static Expression<Func<TEntity, IEnumerable<TSourceEntity>>> BuildIncludeLambda(
        ISkipNavigation skipNavigation,
        IReadOnlyList<IProperty> keyProperties,
        ValueBuffer keyValues)
    {
        var whereParameter = Expression.Parameter(typeof(TSourceEntity), "e");
        var entityParameter = Expression.Parameter(typeof(TEntity), "e");
        return Expression.Lambda<Func<TEntity, IEnumerable<TSourceEntity>>>(
            Expression.Call(
                EnumerableMethods.Where.MakeGenericMethod(typeof(TSourceEntity)),
                Expression.Call(
                    EF.PropertyMethod.MakeGenericMethod(skipNavigation.ClrType),
                    entityParameter,
                    Expression.Constant(skipNavigation.Name, typeof(string))),
                Expression.Lambda<Func<TSourceEntity, bool>>(
                    ExpressionExtensions.BuildPredicate(keyProperties, keyValues, whereParameter),
                    whereParameter)), entityParameter);
    }

    private static Expression<Func<TSourceEntity, bool>> BuildWhereLambda(
        IReadOnlyList<IProperty> keyProperties,
        ValueBuffer keyValues)
    {
        var entityParameter = Expression.Parameter(typeof(TSourceEntity), "e");

        return Expression.Lambda<Func<TSourceEntity, bool>>(
            ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
    }

    private static Expression<Func<TSourceEntity, IEnumerable<TEntity>>> BuildSelectManyLambda(INavigationBase navigation)
    {
        var entityParameter = Expression.Parameter(typeof(TSourceEntity), "e");
        return Expression.Lambda<Func<TSourceEntity, IEnumerable<TEntity>>>(
            Expression.MakeMemberAccess(
                entityParameter,
                navigation.GetIdentifyingMemberInfo()!),
            entityParameter);
    }
}
