// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a given property.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <seealso href="https://aka.ms/efcore-docs-entity-entries">Documentation for accessing tracked entities in EF Core.</seealso>
    public class PropertyEntry : MemberEntry
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyEntry(InternalEntityEntry internalEntry, string name)
            : this(internalEntry, internalEntry.EntityType.GetProperty(name))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyEntry(InternalEntityEntry internalEntry, IProperty property)
            : base(internalEntry, property)
        {
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property has been modified
        ///     and should be updated in the database when <see cref="DbContext.SaveChanges()" />
        ///     is called.
        /// </summary>
        public override bool IsModified
        {
            get => InternalEntry.IsModified(Metadata);
            set => InternalEntry.SetPropertyModified(Metadata, changeState: true, isModified: value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property is considered a
        ///     temporary value which will be replaced by a value generated from the store when
        ///     <see cref="DbContext.SaveChanges()" />is called.
        /// </summary>
        public virtual bool IsTemporary
        {
            get => InternalEntry.HasTemporaryValue(Metadata);
            set
            {
                InternalEntry[Metadata] = CurrentValue;
                InternalEntry.MarkAsTemporary(Metadata, value);
            }
        }

        /// <summary>
        ///     Gets the metadata that describes the facets of this property and how it maps to the database.
        /// </summary>
        public new virtual IProperty Metadata
            => (IProperty)base.Metadata;

        /// <summary>
        ///     Gets or sets the value that was assigned to this property when it was retrieved from the database.
        ///     This property is populated when an entity is retrieved from the database, but setting it may be
        ///     useful in disconnected scenarios where entities are retrieved with one context instance and
        ///     saved with a different context instance.
        /// </summary>
        public virtual object? OriginalValue
        {
            get => InternalEntry.GetOriginalValue(Metadata);
            set => InternalEntry.SetOriginalValue(Metadata, value);
        }
    }
}
