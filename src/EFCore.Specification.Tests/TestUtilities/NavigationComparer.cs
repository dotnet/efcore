// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public class NavigationComparer : IEqualityComparer<INavigation>, IComparer<INavigation>
    {
        private readonly bool _compareAnnotations;

        public NavigationComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(INavigation x, INavigation y) => StringComparer.Ordinal.Compare(x.Name, y.Name);

        public bool Equals(INavigation x, INavigation y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            return x.Name == y.Name
                   && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(INavigation obj) => obj.Name.GetHashCode();
    }
}
