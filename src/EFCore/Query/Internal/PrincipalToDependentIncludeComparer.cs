// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PrincipalToDependentIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _principalKeyValue;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PrincipalToDependentIncludeComparer(
            [NotNull] TKey principalKeyValue,
            [NotNull] IDependentKeyValueFactory<TKey> dependentKeyValueFactory,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _principalKeyValue = principalKeyValue;
            _dependentKeyValueFactory = dependentKeyValueFactory;
            _equalityComparer = principalKeyValueFactory.EqualityComparer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldInclude(in ValueBuffer valueBuffer)
            => _dependentKeyValueFactory.TryCreateFromBuffer(valueBuffer, out var key)
               && _equalityComparer.Equals(key, _principalKeyValue);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldInclude(InternalEntityEntry internalEntityEntry)
            => _dependentKeyValueFactory.TryCreateFromCurrentValues(internalEntityEntry, out var key)
               && _equalityComparer.Equals(key, _principalKeyValue);
    }
}
