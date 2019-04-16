// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyListComparer : IComparer<IReadOnlyList<IProperty>>, IEqualityComparer<IReadOnlyList<IProperty>>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly PropertyListComparer Instance = new PropertyListComparer();

        private PropertyListComparer()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public int Compare(IReadOnlyList<IProperty> x, IReadOnlyList<IProperty> y)
        {
            var result = x.Count - y.Count;

            if (result != 0)
            {
                return result;
            }

            var index = 0;
            while ((result == 0)
                   && (index < x.Count))
            {
                result = StringComparer.Ordinal.Compare(x[index].Name, y[index].Name);
                index++;
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool Equals(IReadOnlyList<IProperty> x, IReadOnlyList<IProperty> y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public int GetHashCode(IReadOnlyList<IProperty> obj)
            => obj.Aggregate(0, (hash, p) => unchecked((hash * 397) ^ p.GetHashCode()));
    }
}
