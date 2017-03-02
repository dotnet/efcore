// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PrincipalToDependentIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _principalKeyValue;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldInclude(ValueBuffer valueBuffer)
        {
            TKey key;
            return _dependentKeyValueFactory.TryCreateFromBuffer(valueBuffer, out key)
                   && _equalityComparer.Equals(key, _principalKeyValue);
        }
    }
}
