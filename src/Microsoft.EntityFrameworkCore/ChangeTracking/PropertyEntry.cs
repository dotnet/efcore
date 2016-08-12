// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    public class PropertyEntry : MemberEntry
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : this(internalEntry, GetProperty(internalEntry, name))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] IProperty property)
            : base(internalEntry, property)
        {
        }

        private static IProperty GetProperty(InternalEntityEntry internalEntry, string name)
        {
            var property = internalEntry.EntityType.FindProperty(name);
            if (property == null)
            {
                if (internalEntry.EntityType.FindNavigation(name) != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyIsNavigation(name, internalEntry.EntityType.DisplayName(),
                            nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)));
                }
                throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, internalEntry.EntityType.DisplayName()));
            }
            return property;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property has been modified
        ///     and should be updated in the database when <see cref="DbContext.SaveChanges()" />
        ///     is called.
        /// </summary>
        public override bool IsModified
        {
            get { return InternalEntry.IsModified(Metadata); }
            set { InternalEntry.SetPropertyModified(Metadata, changeState: true, isModified: value); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property is considered a
        ///     temporary value which will be replaced by a value generated from the store when
        ///     <see cref="DbContext.SaveChanges()" />is called.
        /// </summary>
        public virtual bool IsTemporary
        {
            get { return InternalEntry.HasTemporaryValue(Metadata); }
            set { InternalEntry.MarkAsTemporary(Metadata, value); }
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
        public virtual object OriginalValue
        {
            get { return InternalEntry.GetOriginalValue(Metadata); }
            [param: CanBeNull] set { InternalEntry.SetOriginalValue(Metadata, value); }
        }
    }
}
