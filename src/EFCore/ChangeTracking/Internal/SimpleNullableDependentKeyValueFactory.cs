// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SimpleNullableDependentKeyValueFactory<TKey> : IDependentKeyValueFactory<TKey>
        where TKey : struct
    {
        private readonly PropertyAccessors _propertyAccessors;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SimpleNullableDependentKeyValueFactory(
            IProperty property,
            PropertyAccessors propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
            EqualityComparer = property.CreateKeyEqualityComparer<TKey>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEqualityComparer<TKey> EqualityComparer { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, out TKey key)
        {
            var value = _propertyAccessors.ValueBufferGetter!(valueBuffer);
            if (value == null)
            {
                key = default;
                return false;
            }

            key = (TKey)value;
            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryCreateFromCurrentValues(IUpdateEntry entry, out TKey key)
            => HandleNullableValue(((Func<IUpdateEntry, TKey?>)_propertyAccessors.CurrentValueGetter)(entry), out key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, out TKey key)
            => HandleNullableValue(
                ((Func<IUpdateEntry, TKey?>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry), out key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryCreateFromOriginalValues(IUpdateEntry entry, out TKey key)
            => HandleNullableValue(((Func<IUpdateEntry, TKey?>)_propertyAccessors.OriginalValueGetter!)(entry), out key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, out TKey key)
            => HandleNullableValue(((Func<IUpdateEntry, TKey?>)_propertyAccessors.RelationshipSnapshotGetter)(entry), out key);

        private static bool HandleNullableValue(TKey? value, out TKey key)
        {
            if (value.HasValue)
            {
                key = (TKey)value;
                return true;
            }

            key = default;
            return false;
        }
    }
}
