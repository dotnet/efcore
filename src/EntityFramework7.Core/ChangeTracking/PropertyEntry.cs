// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
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
    public class PropertyEntry
    {
        private readonly InternalEntityEntry _internalEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyEntry" /> class. Instances of this class
        ///     are returned from methods when using the <see cref="ChangeTracker" /> API and it is not designed
        ///     to be directly constructed in your application code.
        /// </summary>
        /// <param name="internalEntry">  The internal entry tracking information about the entity the property belongs to. </param>
        /// <param name="name"> The name of the property. </param>
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotEmpty(name, nameof(name));

            _internalEntry = internalEntry;
            Metadata = internalEntry.EntityType.GetProperty(name);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property has been modified
        ///     and should be updated in the database when <see cref="DbContext.SaveChanges()" />
        ///     is called.
        /// </summary>
        public virtual bool IsModified
        {
            get { return _internalEntry.IsPropertyModified(Metadata); }
            set { _internalEntry.SetPropertyModified(Metadata, value); }
        }

        /// <summary>
        ///     Gets the metadata the context is using to reason about this property.
        /// </summary>
        public virtual IProperty Metadata { get; }

        /// <summary>
        ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
        ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
        ///     for the context to be aware of the change.
        /// </summary>
        public virtual object CurrentValue
        {
            get { return _internalEntry[Metadata]; }
            [param: CanBeNull] set { _internalEntry[Metadata] = value; }
        }

        /// <summary>
        ///     Gets or sets the value that was assigned to this property when it was retrieved from the database.
        ///     This property is populated when an entity is retrieved from the database, but setting it may be
        ///     useful in disconnected scenarios where entities are retrieved with one context instance and
        ///     saved with a different context instance.
        /// </summary>
        public virtual object OriginalValue
        {
            get { return _internalEntry.OriginalValues[Metadata]; }
            [param: CanBeNull] set { _internalEntry.OriginalValues[Metadata] = value; }
        }
    }
}
