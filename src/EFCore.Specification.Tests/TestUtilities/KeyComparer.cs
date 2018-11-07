// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
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

            return y == null
                ? false
                : PropertyListComparer.Instance.Equals(x.Properties, y.Properties)
                   && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IKey obj) => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
