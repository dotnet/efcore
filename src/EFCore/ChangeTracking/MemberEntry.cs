// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a given property
    ///         or navigation property.
    ///     </para>
    ///     <para>
    ///         Scalar properties use the derived class <see cref="PropertyEntry" />, reference navigation
    ///         properties use the derived class <see cref="ReferenceEntry" />, and collection navigation
    ///         properties use the derived class <see cref="CollectionEntry" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public abstract class MemberEntry : IInfrastructure<InternalEntityEntry>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected MemberEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] IPropertyBase metadata)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotNull(metadata, nameof(metadata));

            InternalEntry = internalEntry;
            Metadata = metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalEntityEntry InternalEntry { get; }

        /// <summary>
        ///     <para>
        ///         For non-navigation properties, gets or sets a value indicating whether the value of this
        ///         property has been modified and should be updated in the database when
        ///         <see cref="DbContext.SaveChanges()" />
        ///         is called.
        ///     </para>
        ///     <para>
        ///         For navigation properties, gets or sets a value indicating whether any of foreign key
        ///         property values associated with this navigation property have been modified and should
        ///         be updated in the database  when <see cref="DbContext.SaveChanges()" /> is called.
        ///     </para>
        /// </summary>
        public abstract bool IsModified { get; set; }

        /// <summary>
        ///     Gets the metadata that describes the facets of this property and how it maps to the database.
        /// </summary>
        public virtual IPropertyBase Metadata { get; }

        /// <summary>
        ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
        ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
        ///     for the context to detect the change.
        /// </summary>
        public virtual object CurrentValue
        {
            get => InternalEntry[Metadata];
            [param: CanBeNull] set => InternalEntry[Metadata] = value;
        }

        /// <summary>
        ///     The <see cref="EntityEntry" /> to which this member belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this member. </value>
        public virtual EntityEntry EntityEntry => new EntityEntry(InternalEntry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        InternalEntityEntry IInfrastructure<InternalEntityEntry>.Instance => InternalEntry;

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
