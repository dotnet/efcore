// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
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

            return y == null
                ? false
                : PropertyListComparer.Instance.Equals(x.Properties, y.Properties)
                   && x.IsUnique == y.IsUnique
                   && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IIndex obj) => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
