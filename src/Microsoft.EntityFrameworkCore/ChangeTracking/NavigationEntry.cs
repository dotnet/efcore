// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        private IEntityFinderSource _entityFinderSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected NavigationEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name, bool collection)
            : this(internalEntry, GetNavigation(internalEntry, name, collection))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected NavigationEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] INavigation navigation)
            : base(internalEntry, navigation)
        {
        }

        private static INavigation GetNavigation(InternalEntityEntry internalEntry, string name, bool collection)
        {
            var navigation = internalEntry.EntityType.FindNavigation(name);
            if (navigation == null)
            {
                if (internalEntry.EntityType.FindProperty(name) != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationIsProperty(name, internalEntry.EntityType.DisplayName(),
                            nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)));
                }
                throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, internalEntry.EntityType.DisplayName()));
            }

            if (collection
                && !navigation.IsCollection())
            {
                throw new InvalidOperationException(
                    CoreStrings.CollectionIsReference(name, internalEntry.EntityType.DisplayName(),
                        nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)));
            }

            if (!collection
                && navigation.IsCollection())
            {
                throw new InvalidOperationException(
                    CoreStrings.ReferenceIsCollection(name, internalEntry.EntityType.DisplayName(),
                        nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)));
            }

            return navigation;
        }

        /// <summary>
        ///     <para>
        ///         Loads the entity or entities referenced by this navigation property.
        ///     </para>
        ///     <para>
        ///         Note that entities that are already being tracked are not overwritten with new data from the database.
        ///     </para>
        /// </summary>
        public virtual void Load()
            => Finder(Metadata.GetTargetType().ClrType).Load(GetLoadProperties(), GetLoadValues());

        /// <summary>
        ///     <para>
        ///         Loads the entity or entities referenced by this navigation property.
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
        ///     A task that represents the asynchronous save operation.
        /// </returns>
        public virtual Task LoadAsync(CancellationToken cancellationToken = new CancellationToken())
            => Finder(Metadata.GetTargetType().ClrType)
                .LoadAsync(GetLoadProperties(), GetLoadValues(), cancellationToken);

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
        public virtual IQueryable Query()
            => Finder(Metadata.GetTargetType().ClrType).Query(GetLoadProperties(), GetLoadValues());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual object[] GetLoadValues()
            => (Metadata.IsDependentToPrincipal()
                ? Metadata.ForeignKey.Properties
                : Metadata.ForeignKey.PrincipalKey.Properties)
                .Select(p => InternalEntry[p]).ToArray();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IReadOnlyList<IProperty> GetLoadProperties()
            => Metadata.IsDependentToPrincipal()
                ? Metadata.ForeignKey.PrincipalKey.Properties
                : Metadata.ForeignKey.Properties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEntityFinder Finder([NotNull] Type entityType)
            => (_entityFinderSource
                ?? (_entityFinderSource = InternalEntry.StateManager.Context.GetService<IEntityFinderSource>()))
                .Create(InternalEntry.StateManager.Context, entityType);

        /// <summary>
        ///     Gets or sets a value indicating whether any of foreign key property values associated
        ///     with this navigation property have been modified and should be updated in the database
        ///     when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        public override bool IsModified
        {
            get
            {
                if (Metadata.IsDependentToPrincipal())
                {
                    return AnyFkPropertiesModified(InternalEntry);
                }

                var navigationValue = CurrentValue;

                return navigationValue != null
                       && (Metadata.IsCollection()
                           ? ((IEnumerable)navigationValue).OfType<object>().Any(AnyFkPropertiesModified)
                           : AnyFkPropertiesModified(navigationValue));
            }
            set
            {
                if (Metadata.IsDependentToPrincipal())
                {
                    SetFkPropertiesModified(InternalEntry, value);
                }
                else
                {
                    var navigationValue = CurrentValue;
                    if (navigationValue != null)
                    {
                        if (Metadata.IsCollection())
                        {
                            foreach (var relatedEntity in (IEnumerable)navigationValue)
                            {
                                SetFkPropertiesModified(relatedEntity, value);
                            }
                        }
                        else
                        {
                            SetFkPropertiesModified(navigationValue, value);
                        }
                    }
                }
            }
        }

        private bool AnyFkPropertiesModified(object relatedEntity)
        {
            var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity);

            return relatedEntry != null
                   && Metadata.ForeignKey.Properties.Any(relatedEntry.IsModified);
        }

        private void SetFkPropertiesModified(object relatedEntity, bool modified)
        {
            var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity);
            if (relatedEntry != null)
            {
                SetFkPropertiesModified(relatedEntry, modified);
            }
        }

        private void SetFkPropertiesModified(InternalEntityEntry internalEntityEntry, bool modified)
        {
            foreach (var property in Metadata.ForeignKey.Properties)
            {
                internalEntityEntry.SetPropertyModified(property, isModified: modified);
            }
        }

        private bool AnyFkPropertiesModified(InternalEntityEntry internalEntityEntry)
            => Metadata.ForeignKey.Properties.Any(internalEntityEntry.IsModified);

        /// <summary>
        ///     Gets the metadata that describes the facets of this property and how it maps to the database.
        /// </summary>
        public new virtual INavigation Metadata
            => (INavigation)base.Metadata;
    }
}
