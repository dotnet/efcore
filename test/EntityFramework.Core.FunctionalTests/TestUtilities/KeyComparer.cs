// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public class KeyComparer : IEqualityComparer<IKey>, IComparer<IKey>
    {
        private readonly bool _compareAnnotations;

        public KeyComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IKey x, IKey y) => PropertyListComparer.Instance.Compare(x.Properties, y.Properties);

        public bool Equals(IKey x, IKey y)
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
                   && (!_compareAnnotations || x.Annotations.SequenceEqual(y.Annotations, AnnotationComparer.Instance));
        }

        public int GetHashCode(IKey obj) => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
