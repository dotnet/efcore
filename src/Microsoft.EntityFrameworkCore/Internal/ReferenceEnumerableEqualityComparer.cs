// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public sealed class ReferenceEnumerableEqualityComparer<TEnumerable, TValue> : IEqualityComparer<TEnumerable>
        where TEnumerable : IEnumerable<TValue>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool Equals(TEnumerable x, TEnumerable y) => x.SequenceEqual(y);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public int GetHashCode(TEnumerable obj) => obj.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode());
    }
}
