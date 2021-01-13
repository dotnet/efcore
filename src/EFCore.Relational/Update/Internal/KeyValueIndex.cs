// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class KeyValueIndex<TKey> : IKeyValueIndex
    {
        private readonly IForeignKey _foreignKey;
        private readonly TKey _keyValue;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly bool _fromOriginalValues;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public KeyValueIndex(
            [CanBeNull] IForeignKey foreignKey,
            [NotNull] TKey keyValue,
            [NotNull] IEqualityComparer<TKey> keyComparer,
            bool fromOriginalValues)
        {
            _foreignKey = foreignKey;
            _keyValue = keyValue;
            _fromOriginalValues = fromOriginalValues;
            _keyComparer = keyComparer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IKeyValueIndex WithOriginalValuesFlag()
            => new KeyValueIndex<TKey>(_foreignKey, _keyValue, _keyComparer, fromOriginalValues: true);

        private bool Equals(KeyValueIndex<TKey> other)
            => other._fromOriginalValues == _fromOriginalValues
                && other._foreignKey == _foreignKey
                && _keyComparer.Equals(_keyValue, other._keyValue);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Equals(object obj)
            => !(obj is null)
                && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType()
                    && Equals((KeyValueIndex<TKey>)obj));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(typeof(TKey));
            hash.Add(_fromOriginalValues);
            hash.Add(_foreignKey);
            hash.Add(_keyValue, _keyComparer);
            return hash.ToHashCode();
        }
    }
}
