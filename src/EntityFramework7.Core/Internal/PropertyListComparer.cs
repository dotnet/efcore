// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public class PropertyListComparer : IComparer<IReadOnlyList<IProperty>>, IEqualityComparer<IReadOnlyList<IProperty>>
    {
        public static readonly PropertyListComparer Instance = new PropertyListComparer();

        private PropertyListComparer()
        {
        }

        public int Compare(IReadOnlyList<IProperty> x, IReadOnlyList<IProperty> y)
        {
            var result = x.Count - y.Count;

            if (result != 0)
            {
                return result;
            }

            var index = 0;
            while (result == 0
                   && index < x.Count)
            {
                result = StringComparer.Ordinal.Compare(x[index].Name, y[index].Name);
                index++;
            }
            return result;
        }

        public bool Equals(IReadOnlyList<IProperty> x, IReadOnlyList<IProperty> y)
            => Compare(x, y) == 0;

        public int GetHashCode(IReadOnlyList<IProperty> obj)
            => obj.Aggregate(0, (hash, p) => unchecked((hash * 397) ^ p.GetHashCode()));
    }
}
