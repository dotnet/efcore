// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Internal
{
    public sealed class ReferenceEnumerableEqualityComparer<TEnumerable, TValue> : IEqualityComparer<TEnumerable>
        where TEnumerable : IEnumerable<TValue>
    {
        public bool Equals(TEnumerable left, TEnumerable right) => left.SequenceEqual(right);

        public int GetHashCode(TEnumerable values) => values.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode());
    }
}
