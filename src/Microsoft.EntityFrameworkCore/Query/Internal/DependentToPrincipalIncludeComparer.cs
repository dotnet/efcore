// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class DependentToPrincipalIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _dependentKeyValue;
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        public DependentToPrincipalIncludeComparer(
            [NotNull] TKey dependentKeyValue,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _dependentKeyValue = dependentKeyValue;
            _principalKeyValueFactory = principalKeyValueFactory;
            _equalityComparer = principalKeyValueFactory.EqualityComparer;
        }

        public virtual bool ShouldInclude(ValueBuffer valueBuffer)
            => _equalityComparer.Equals(
                (TKey)_principalKeyValueFactory.CreateFromBuffer(valueBuffer),
                _dependentKeyValue);
    }
}
