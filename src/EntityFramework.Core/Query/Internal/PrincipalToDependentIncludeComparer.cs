// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class PrincipalToDependentIncludeComparer<TKey> : IIncludeKeyComparer
    {
        private readonly TKey _principalKeyValue;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;

        public PrincipalToDependentIncludeComparer(
            [NotNull] TKey principalKeyValue,
            [NotNull] IDependentKeyValueFactory<TKey> dependentKeyValueFactory)
        {
            _principalKeyValue = principalKeyValue;
            _dependentKeyValueFactory = dependentKeyValueFactory;
        }

        public virtual bool ShouldInclude(ValueBuffer valueBuffer)
        {
            TKey key;
            return _dependentKeyValueFactory.TryCreateFromBuffer(valueBuffer, out key)
                   && key.Equals(_principalKeyValue);
        }
    }
}
