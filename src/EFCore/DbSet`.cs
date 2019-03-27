// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         A <see cref="DbSet{TEntity}" /> can be used to query and save instances of <typeparamref name="TEntity" />.
    ///         LINQ queries against a <see cref="DbSet{TEntity}" /> will be translated into queries against the database.
    ///     </para>
    ///     <para>
    ///         The results of a LINQ query against a <see cref="DbSet{TEntity}" /> will contain the results
    ///         returned from the database and may not reflect changes made in the context that have not
    ///         been persisted to the database. For example, the results will not contain newly added entities
    ///         and may still contain entities that are marked for deletion.
    ///     </para>
    ///     <para>
    ///         Depending on the database being used, some parts of a LINQ query against a <see cref="DbSet{TEntity}" />
    ///         may be evaluated in memory rather than being translated into a database query.
    ///     </para>
    ///     <para>
    ///         <see cref="DbSet{TEntity}" /> objects are usually obtained from a <see cref="DbSet{TEntity}" />
    ///         property on a derived <see cref="DbContext" /> or from the <see cref="DbContext.Set{TEntity}" />
    ///         method.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The type of entity being operated on by this set. </typeparam>
    public abstract class DbSet<TEntity>
        : IQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>, IInfrastructure<IServiceProvider>, IListSource
        where TEntity : class
    {
        /// <summary>
        ///     <para>
        ///         Gets an <see cref="LocalView{TEntity}" /> that represents a local view of all Added, Unchanged,
        ///         and Modified entities in this set.
        ///     </para>
        ///     <para>
        ///         This local view will stay in sync as entities are added or removed from the context. Likewise, entities
        ///         added to or removed from the local view will automatically be added to or removed
        ///         from the context.
        ///     </para>
        ///     <para>
        ///         This property can be used for data binding by populating the set with data, for example by using the
        ///         <see cref="EntityFrameworkQueryableExtensions.Load{TSource}" /> extension method,
        ///         and then binding to the local data through this property by calling
        ///         <see cref="LocalView{TEntity}.ToObservableCollection" /> for WPF binding, or
        ///         <see cref="LocalView{TEntity}.ToBindingList" /> for WinForms.
        ///     </para>
        /// </summary>
        public virtual LocalView<TEntity> Local => throw new NotImplementedException();

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or null.</returns>
        public virtual TEntity Find([CanBeNull] params object[] keyValues) => throw new NotImplementedException();

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or null.</returns>
        public virtual Task<TEntity> FindAsync([CanBeNull] params object[] keyValues) => throw new NotImplementedException();

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The entity found, or null.</returns>
        public virtual Task<TEntity> FindAsync([CanBeNull] object[] keyValues, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Add([NotNull] TEntity entity) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides access to change tracking
        ///     information and operations for the entity.
        /// </returns>
        public virtual Task<EntityEntry<TEntity>> AddAsync(
            [NotNull] TEntity entity,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity in the <see cref="EntityState.Unchanged" /> state
        ///         such that no operation will be performed when <see cref="DbContext.SaveChanges()" />
        ///         is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Attach([NotNull] TEntity entity) => throw new NotImplementedException();

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Attach(TEntity)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Remove([NotNull] TEntity entity) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity in the <see cref="EntityState.Modified" /> state such that it will
        ///         be updated in the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of the entity will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(TEntity)" /> to begin tracking the entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Update([NotNull] TEntity entity) => throw new NotImplementedException();

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange([NotNull] params TEntity[] entities) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual Task AddRangeAsync([NotNull] params TEntity[] entities) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Unchanged" /> state
        ///         such that no operation will be performed when <see cref="DbContext.SaveChanges()" />
        ///         is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange([NotNull] params TEntity[] entities) => throw new NotImplementedException();

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Deleted" /> state such that they will
        ///     be removed from the database when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="AttachRange(TEntity[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange([NotNull] params TEntity[] entities) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Modified" /> state such that they will
        ///         be updated in the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of each entity will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(TEntity)" /> to begin tracking each entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange([NotNull] params TEntity[] entities) => throw new NotImplementedException();

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange([NotNull] IEnumerable<TEntity> entities) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual Task AddRangeAsync(
            [NotNull] IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Unchanged" /> state
        ///         such that no operation will be performed when <see cref="DbContext.SaveChanges()" />
        ///         is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange([NotNull] IEnumerable<TEntity> entities) => throw new NotImplementedException();

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Deleted" /> state such that they will
        ///     be removed from the database when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="AttachRange(IEnumerable{TEntity})" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange([NotNull] IEnumerable<TEntity> entities) => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Modified" /> state such that they will
        ///         be updated in the database when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of each entity will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(TEntity)" /> to begin tracking each entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. These entities will also begin to be tracked
        ///         by the context. If a reachable entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange([NotNull] IEnumerable<TEntity> entities) => throw new NotImplementedException();

        /// <summary>
        ///     Returns an <see cref="IEnumerator{T}" /> which when enumerated will execute a query against the database
        ///     to load all entities from the database.
        /// </summary>
        /// <returns> The query results. </returns>
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => throw new NotImplementedException();

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute a query against the database
        ///     to load all entities from the database.
        /// </summary>
        /// <returns> The query results. </returns>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        /// <summary>
        ///     Returns an <see cref="IAsyncEnumerable{T}" /> which when enumerated will asynchronously execute the query against
        ///     the database.
        /// </summary>
        /// <returns> The query results. </returns>
        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => throw new NotImplementedException();

        /// <summary>
        ///     Gets the IQueryable element type.
        /// </summary>
        Type IQueryable.ElementType => throw new NotImplementedException();

        /// <summary>
        ///     Gets the IQueryable LINQ Expression.
        /// </summary>
        Expression IQueryable.Expression => throw new NotImplementedException();

        /// <summary>
        ///     Gets the IQueryable provider.
        /// </summary>
        IQueryProvider IQueryable.Provider => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         Gets the scoped <see cref="IServiceProvider" /> being used to resolve services.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods that need to make use of services
        ///         not directly exposed in the public API surface.
        ///     </para>
        /// </summary>
        IServiceProvider IInfrastructure<IServiceProvider>.Instance => throw new NotImplementedException();

        /// <summary>
        ///     <para>
        ///         This method is called by data binding frameworks when attempting to data bind
        ///         directly to a <see cref="DbSet{TEntity}" />.
        ///     </para>
        ///     <para>
        ///         This implementation always throws an exception as binding directly to a
        ///         <see cref="DbSet{TEntity}" /> will result in a query being
        ///         sent to the database every time the data binding framework requests the contents
        ///         of the collection. Instead load the results into the context, for example, by using the
        ///         <see cref="EntityFrameworkQueryableExtensions.Load{TSource}" /> extension method,
        ///         and then bind to the local data through the <see cref="Local" /> by calling
        ///         <see cref="LocalView{TEntity}.ToObservableCollection" /> for WPF binding, or
        ///         <see cref="LocalView{TEntity}.ToBindingList" /> for WinForms.
        ///     </para>
        /// </summary>
        /// <exception cref="NotSupportedException"> Always thrown. </exception>
        /// <returns> Never returns, always throws an exception. </returns>
        IList IListSource.GetList()
        {
            throw new NotSupportedException(CoreStrings.DataBindingWithIListSource);
        }

        /// <summary>
        ///     Gets a value indicating whether the collection is a collection of System.Collections.IList objects.
        ///     Always returns false.
        /// </summary>
        bool IListSource.ContainsListCollection => false;

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
