// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
    /// <typeparam name="TEntity"> The type of the entity the property belongs to. </typeparam>
    /// <typeparam name="TProperty"> The type of the property. </typeparam>
    public class PropertyEntry<TEntity, TProperty> : PropertyEntry
        where TEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] IProperty property)
            : base(internalEntry, property)
        {
        }

        /// <summary>
        ///     The <see cref="EntityEntry{TEntity}" /> to which this member belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this member. </value>
        public new virtual EntityEntry<TEntity> EntityEntry
            => new EntityEntry<TEntity>(InternalEntry);

        /// <inheritdoc cref="MemberEntry.CurrentValue" />
        public new virtual TProperty CurrentValue
        {
            get => InternalEntry.GetCurrentValue<TProperty>(Metadata);
            [param: CanBeNull] set => base.CurrentValue = value;
        }

        /// <inheritdoc cref="PropertyEntry.OriginalValue" />
        public new virtual TProperty OriginalValue
        {
            get => InternalEntry.GetOriginalValue<TProperty>(Metadata);
            [param: CanBeNull] set => base.OriginalValue = value;
        }
    }
}
