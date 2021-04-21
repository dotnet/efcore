// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestIndexComparer : IEqualityComparer<IReadOnlyIndex>, IComparer<IReadOnlyIndex>
    {
        private readonly bool _compareAnnotations;

        public TestIndexComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IReadOnlyIndex x, IReadOnlyIndex y)
            => PropertyListComparer.Instance.Compare(x.Properties, y.Properties);

        public bool Equals(IReadOnlyIndex x, IReadOnlyIndex y)
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

        public int GetHashCode(IReadOnlyIndex obj)
            => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
