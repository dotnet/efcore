// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class PrincipalToDependentIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _principalKeyValue;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        public PrincipalToDependentIncludeComparer(
            [NotNull] TKey principalKeyValue,
            [NotNull] IDependentKeyValueFactory<TKey> dependentKeyValueFactory,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _principalKeyValue = principalKeyValue;
            _dependentKeyValueFactory = dependentKeyValueFactory;
            _equalityComparer = principalKeyValueFactory.EqualityComparer;
        }

        public virtual bool ShouldInclude(ValueBuffer valueBuffer)
        {
            TKey key;
            return _dependentKeyValueFactory.TryCreateFromBuffer(valueBuffer, out key)
                   && _equalityComparer.Equals(key, _principalKeyValue);
        }
    }
}
