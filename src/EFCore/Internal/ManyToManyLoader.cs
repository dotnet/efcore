// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ValueBuffer = Microsoft.EntityFrameworkCore.Storage.ValueBuffer;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ManyToManyLoader<TEntity, TSourceEntity, TJoinEntity> : ICollectionLoader<TEntity>
    where TEntity : class
    where TSourceEntity : class
    where TJoinEntity : class
{
    private readonly ISkipNavigation _skipNavigation;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ManyToManyLoader(ISkipNavigation skipNavigation)
    {
        _skipNavigation = skipNavigation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Load(InternalEntityEntry entry)
    {
        var keyValues = PrepareForLoad(entry);

        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            Query(entry.Context, keyValues).Result.Load();
        }

        entry.SetIsLoaded(_skipNavigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task LoadAsync(InternalEntityEntry entry, CancellationToken cancellationToken = default)
    {
        var keyValues = PrepareForLoad(entry);

        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            await (await Query(entry.Context, keyValues, true, cancellationToken).ConfigureAwait(false))
                .LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        entry.SetIsLoaded(_skipNavigation);
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

        return Query(context, keyValues).Result;
    }
    
    private object[]? PrepareForLoad(InternalEntityEntry entry)
    {
        if (entry.EntityState == EntityState.Detached)
        {
            throw new InvalidOperationException(CoreStrings.CannotLoadDetached(_skipNavigation.Name, entry.EntityType.DisplayName()));
        }

        var properties = _skipNavigation.ForeignKey.PrincipalKey.Properties;
        var values = new object[properties.Count];

        for (var i = 0; i < values.Length; i++)
        {
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

    private async Task<IQueryable<TEntity>> Query(
        DbContext context, object[] keyValues, bool async = false, CancellationToken cancellationToken = default)
    {
        // Example of query being built:
        //
        // IQueryable<EntityTwo> loaded
        //     = context.Set<EntityOne>()
        //        .AsTracking()
        //        .Where(e => e.Id == left.Id)
        //        .SelectMany(e => e.TwoSkip)
        //        .NotQuiteInclude(e => e.OneSkip.Where(e => e.Id == left.Id));

        var loadProperties = _skipNavigation.ForeignKey.PrincipalKey.Properties;
        var queryRoot = _skipNavigation.DeclaringEntityType.HasSharedClrType
            ? context.Set<TSourceEntity>(_skipNavigation.DeclaringEntityType.Name)
            : context.Set<TSourceEntity>();

        var queryable = queryRoot
            .AsTracking()
            .Where(BuildWhereLambda<TSourceEntity>(loadProperties, new ValueBuffer(keyValues)))
            .SelectMany(BuildSelectManyLambda(_skipNavigation));
        
        if (_skipNavigation.Inverse.IsShadowProperty())
        {
            // TODOU: This is a hack, until #27493 is implemented.
            var joinEntitiesQuery = (_skipNavigation.JoinEntityType.HasSharedClrType
                    ? context.Set<TJoinEntity>(_skipNavigation.JoinEntityType.Name)
                    : context.Set<TJoinEntity>())
                .AsTracking()
                .Where(BuildWhereLambda<TJoinEntity>(_skipNavigation.ForeignKey.Properties, new ValueBuffer(keyValues)));

            if (async)
            {
                await joinEntitiesQuery.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                joinEntitiesQuery.Load();
            }

            return queryable;
        }
        
        return queryable.NotQuiteInclude(BuildIncludeLambda(_skipNavigation.Inverse, loadProperties, new ValueBuffer(keyValues)));
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
                Expression.MakeMemberAccess(
                    entityParameter,
                    skipNavigation.GetIdentifyingMemberInfo()!),
                Expression.Lambda<Func<TSourceEntity, bool>>(
                    ExpressionExtensions.BuildPredicate(keyProperties, keyValues, whereParameter),
                    whereParameter)), entityParameter);
    }

    private static Expression<Func<T, bool>> BuildWhereLambda<T>(
        IReadOnlyList<IProperty> keyProperties,
        ValueBuffer keyValues)
    {
        var entityParameter = Expression.Parameter(typeof(T), "e");

        return Expression.Lambda<Func<T, bool>>(
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
