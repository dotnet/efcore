// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public class IndexComparer : IEqualityComparer<IIndex>, IComparer<IIndex>
    {
        private readonly bool _compareAnnotations;

        public IndexComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IIndex x, IIndex y) => PropertyListComparer.Instance.Compare(x.Properties, y.Properties);

        public bool Equals(IIndex x, IIndex y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            return PropertyListComparer.Instance.Equals(x.Properties, y.Properties)
                   && x.IsUnique == y.IsUnique
                   && (!_compareAnnotations || x.Annotations.SequenceEqual(y.Annotations, AnnotationComparer.Instance));
        }

        public int GetHashCode(IIndex obj) => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
