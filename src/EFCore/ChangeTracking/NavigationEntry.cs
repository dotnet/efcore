// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking and loading information for a navigation property
    ///         that associates this entity to one or more other entities.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public abstract class NavigationEntry : MemberEntry
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected NavigationEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name, bool collection)
            : this(internalEntry, GetNavigation(internalEntry, name, collection))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected NavigationEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] INavigationBase navigation)
            : base(internalEntry, navigation)
        {
        }

        private static INavigationBase GetNavigation(InternalEntityEntry internalEntry, string name, bool collection)
        {
            var navigation = (INavigationBase)internalEntry.EntityType.FindNavigation(name)
                ?? internalEntry.EntityType.FindSkipNavigation(name);

            if (navigation == null)
            {
                if (internalEntry.EntityType.FindProperty(name) != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationIsProperty(
                            name, internalEntry.EntityType.DisplayName(),
                            nameof(ChangeTracking.EntityEntry.Reference), nameof(ChangeTracking.EntityEntry.Collection),
                            nameof(ChangeTracking.EntityEntry.Property)));
                }

                throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, internalEntry.EntityType.DisplayName()));
            }

            if (collection
                && !navigation.IsCollection)
            {
                throw new InvalidOperationException(
                    CoreStrings.CollectionIsReference(
                        name, internalEntry.EntityType.DisplayName(),
                        nameof(ChangeTracking.EntityEntry.Collection), nameof(ChangeTracking.EntityEntry.Reference)));
            }

            if (!collection
                && navigation.IsCollection)
            {
                throw new InvalidOperationException(
                    CoreStrings.ReferenceIsCollection(
                        name, internalEntry.EntityType.DisplayName(),
                        nameof(ChangeTracking.EntityEntry.Reference), nameof(ChangeTracking.EntityEntry.Collection)));
            }

            return navigation;
        }

        /// <summary>
        ///     <para>
        ///         Loads the entity or entities referenced by this navigation property, unless <see cref="IsLoaded" />
        ///         is already set to true.
        ///     </para>
        ///     <para>
        ///         Note that entities that are already being tracked are not overwritten with new data from the database.
        ///     </para>
        /// </summary>
        public abstract void Load();

        /// <summary>
        ///     <para>
        ///         Loads the entity or entities referenced by this navigation property, unless <see cref="IsLoaded" />
        ///         is already set to true.
        ///     </para>
        ///     <para>
        ///         Note that entities that are already being tracked are not overwritten with new data from the database.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public abstract Task LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Returns the query that would be used by <see cref="Load" /> to load entities referenced by
        ///         this navigation property.
        ///     </para>
        ///     <para>
        ///         The query can be composed over using LINQ to perform filtering, counting, etc. without
        ///         actually loading all entities from the database.
        ///     </para>
        /// </summary>
        /// <returns> The query to load related entities. </returns>
        public abstract IQueryable Query();

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether the entity or entities referenced by this navigation property
        ///         are known to be loaded.
        ///     </para>
        ///     <para>
        ///         Loading entities from the database using
        ///         <see cref="EntityFrameworkQueryableExtensions.Include{TEntity,TProperty}" /> or
        ///         <see
        ///             cref="EntityFrameworkQueryableExtensions.ThenInclude{TEntity,TPreviousProperty,TProperty}(EntityFrameworkCore.Query.IIncludableQueryable{TEntity,IEnumerable{TPreviousProperty}},System.Linq.Expressions.Expression{System.Func{TPreviousProperty,TProperty}})" />
        ///         , <see cref="Load" />, or <see cref="LoadAsync" /> will set this flag. Subsequent calls to <see cref="Load" />
        ///         or <see cref="LoadAsync" /> will then be a no-op.
        ///     </para>
        ///     <para>
        ///         It is possible for IsLoaded to be false even if all related entities are loaded. This is because, depending on
        ///         how entities are loaded, it is not always possible to know for sure that all entities in a related collection
        ///         have been loaded. In such cases, calling <see cref="Load" /> or <see cref="LoadAsync" /> will ensure all
        ///         related entities are loaded and will set this flag to true.
        ///     </para>
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if all the related entities are loaded or the IsLoaded has been explicitly set to true.
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
}
