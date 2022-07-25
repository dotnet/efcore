// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Sql Server database specific extension methods for LINQ queries rooted in DbSet.
/// </summary>
public static class SqlServerDbSetExtensions
{
    /// <summary>
    ///     Applies temporal 'AsOf' operation on the given DbSet, which only returns elements that were present in the database at a given
    ///     point in time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Temporal information is stored in UTC format on the database, so any <see cref="DateTime" /> arguments in local time may lead to
    ///         unexpected results.
    ///     </para>
    ///     <para>
    ///         Temporal queries are always set as 'NoTracking'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
    /// <param name="utcPointInTime"><see cref="DateTime" /> representing a point in time for which the results should be returned.</param>
    /// <returns>An <see cref="IQueryable" /> representing the entities at a given point in time.</returns>
    public static IQueryable<TEntity> TemporalAsOf<TEntity>(
        this DbSet<TEntity> source,
        DateTime utcPointInTime)
        where TEntity : class
    {
        var queryableSource = (IQueryable)source;
        var entityQueryRootExpression = (EntityQueryRootExpression)queryableSource.Expression;
        var entityType = entityQueryRootExpression.EntityType;

        return queryableSource.Provider.CreateQuery<TEntity>(
            new TemporalAsOfQueryRootExpression(
                entityQueryRootExpression.QueryProvider!,
                entityType,
                utcPointInTime)).AsNoTracking();
    }

    /// <summary>
    ///     Applies temporal 'FromTo' operation on the given DbSet, which only returns elements that were present in the database between two
    ///     points in time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Elements that were created at the starting point as well as elements that were removed at the end point are not included in the
    ///         results.
    ///     </para>
    ///     <para>
    ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities
    ///         with the same key.
    ///     </para>
    ///     <para>
    ///         Temporal information is stored in UTC format on the database, so any <see cref="DateTime" /> arguments in local time may lead to
    ///         unexpected results.
    ///     </para>
    ///     <para>
    ///         Temporal queries are always set as 'NoTracking'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
    /// <param name="utcFrom">Point in time representing the start of the period for which results should be returned.</param>
    /// <param name="utcTo">Point in time representing the end of the period for which results should be returned.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
    public static IQueryable<TEntity> TemporalFromTo<TEntity>(
        this DbSet<TEntity> source,
        DateTime utcFrom,
        DateTime utcTo)
        where TEntity : class
    {
        var queryableSource = (IQueryable)source;
        var entityQueryRootExpression = (EntityQueryRootExpression)queryableSource.Expression;
        var entityType = entityQueryRootExpression.EntityType;

        return queryableSource.Provider.CreateQuery<TEntity>(
            new TemporalFromToQueryRootExpression(
                entityQueryRootExpression.QueryProvider!,
                entityType,
                utcFrom,
                utcTo)).AsNoTracking();
    }

    /// <summary>
    ///     Applies temporal 'Between' operation on the given DbSet, which only returns elements that were present in the database between two
    ///     points in time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Elements that were created at the starting point are not included in the results, however elements that were removed at the end
    ///         point are included in the results.
    ///     </para>
    ///     <para>
    ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities
    ///         with the same key.
    ///     </para>
    ///     <para>
    ///         Temporal information is stored in UTC format on the database, so any <see cref="DateTime" /> arguments in local time may lead to
    ///         unexpected results.
    ///     </para>
    ///     <para>
    ///         Temporal queries are always set as 'NoTracking'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
    /// <param name="utcFrom">Point in time representing the start of the period for which results should be returned.</param>
    /// <param name="utcTo">Point in time representing the end of the period for which results should be returned.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
    public static IQueryable<TEntity> TemporalBetween<TEntity>(
        this DbSet<TEntity> source,
        DateTime utcFrom,
        DateTime utcTo)
        where TEntity : class
    {
        var queryableSource = (IQueryable)source;
        var entityQueryRootExpression = (EntityQueryRootExpression)queryableSource.Expression;
        var entityType = entityQueryRootExpression.EntityType;

        return queryableSource.Provider.CreateQuery<TEntity>(
            new TemporalBetweenQueryRootExpression(
                entityQueryRootExpression.QueryProvider!,
                entityType,
                utcFrom,
                utcTo)).AsNoTracking();
    }

    /// <summary>
    ///     Applies temporal 'ContainedIn' operation on the given DbSet, which only returns elements that were present in the database between
    ///     two points in time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Elements that were created at the starting point as well as elements that were removed at the end point are included in the
    ///         results.
    ///     </para>
    ///     <para>
    ///         All versions of entities in that were present within the time range are returned, so it is possible to return multiple entities
    ///         with the same key.
    ///     </para>
    ///     <para>
    ///         Temporal information is stored in UTC format on the database, so any <see cref="DateTime" /> arguments in local time may lead to
    ///         unexpected results.
    ///     </para>
    ///     <para>
    ///         Temporal queries are always set as 'NoTracking'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
    /// <param name="utcFrom">Point in time representing the start of the period for which results should be returned.</param>
    /// <param name="utcTo">Point in time representing the end of the period for which results should be returned.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the entities present in a given time range.</returns>
    public static IQueryable<TEntity> TemporalContainedIn<TEntity>(
        this DbSet<TEntity> source,
        DateTime utcFrom,
        DateTime utcTo)
        where TEntity : class
    {
        var queryableSource = (IQueryable)source;
        var entityQueryRootExpression = (EntityQueryRootExpression)queryableSource.Expression;
        var entityType = entityQueryRootExpression.EntityType;

        return queryableSource.Provider.CreateQuery<TEntity>(
            new TemporalContainedInQueryRootExpression(
                entityQueryRootExpression.QueryProvider!,
                entityType,
                utcFrom,
                utcTo)).AsNoTracking();
    }

    /// <summary>
    ///     Applies temporal 'All' operation on the given DbSet, which returns all historical versions of the entities as well as their current
    ///     state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Temporal queries are always set as 'NoTracking'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="source">Source DbSet on which the temporal operation is applied.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the entities and their historical versions.</returns>
    public static IQueryable<TEntity> TemporalAll<TEntity>(
        this DbSet<TEntity> source)
        where TEntity : class
    {
        var queryableSource = (IQueryable)source;
        var entityQueryRootExpression = (EntityQueryRootExpression)queryableSource.Expression;
        var entityType = entityQueryRootExpression.EntityType;

        return queryableSource.Provider.CreateQuery<TEntity>(
            new TemporalAllQueryRootExpression(
                entityQueryRootExpression.QueryProvider!, entityType)).AsNoTracking();
    }
}
