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
    public class DependentToPrincipalIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _dependentKeyValue;
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DependentToPrincipalIncludeComparer(
            [NotNull] TKey dependentKeyValue,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _dependentKeyValue = dependentKeyValue;
            _principalKeyValueFactory = principalKeyValueFactory;
            _equalityComparer = principalKeyValueFactory.EqualityComparer;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldInclude(in ValueBuffer valueBuffer)
            => _equalityComparer.Equals(
                (TKey)_principalKeyValueFactory.CreateFromBuffer(valueBuffer),
                _dependentKeyValue);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldInclude(InternalEntityEntry internalEntityEntry)
            => _equalityComparer.Equals(
                _principalKeyValueFactory.CreateFromCurrentValues(internalEntityEntry),
                _dependentKeyValue);
    }
}
