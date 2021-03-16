// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class NavigationComparer : IEqualityComparer<IReadOnlyNavigation>, IComparer<IReadOnlyNavigation>
    {
        private readonly bool _compareAnnotations;

        public NavigationComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IReadOnlyNavigation x, IReadOnlyNavigation y)
            => StringComparer.Ordinal.Compare(x.Name, y.Name);

        public bool Equals(IReadOnlyNavigation x, IReadOnlyNavigation y)
        {
            if (x == null)
            {
                return y == null;
            }

            return y == null
                ? false
                : x.Name == y.Name
                && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IReadOnlyNavigation obj)
            => obj.Name.GetHashCode();
    }
}
