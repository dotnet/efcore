// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestKeyComparer : IEqualityComparer<IReadOnlyKey>, IComparer<IReadOnlyKey>
    {
        private readonly bool _compareAnnotations;

        public TestKeyComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IReadOnlyKey x, IReadOnlyKey y)
            => PropertyListComparer.Instance.Compare(x.Properties, y.Properties);

        public bool Equals(IReadOnlyKey x, IReadOnlyKey y)
        {
            if (x == null)
            {
                return y == null;
            }

            return y == null
                ? false
                : PropertyListComparer.Instance.Equals(x.Properties, y.Properties)
                && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IReadOnlyKey obj)
            => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
