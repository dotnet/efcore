// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ForeignKeyStrictComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        private readonly bool _compareAnnotations;
        private readonly bool _compareNavigations;

        public ForeignKeyStrictComparer(bool compareAnnotations = true, bool compareNavigations = true)
        {
            _compareAnnotations = compareAnnotations;
            _compareNavigations = compareNavigations;
        }

        public int Compare(IForeignKey x, IForeignKey y) => ForeignKeyComparer.Instance.Compare(x, y);

        public bool Equals(IForeignKey x, IForeignKey y)
        {
            if (x == null)
            {
                return y == null;
            }

            return y == null
                ? false
                : ForeignKeyComparer.Instance.Equals(x, y)
                   && (x.IsUnique == y.IsUnique)
                   && (x.IsRequired == y.IsRequired)
                   && (!_compareNavigations
                       || (new NavigationComparer(_compareAnnotations).Equals(x.DependentToPrincipal, y.DependentToPrincipal)
                           && new NavigationComparer(_compareAnnotations).Equals(x.PrincipalToDependent, y.PrincipalToDependent)))
                   && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IForeignKey obj) => ForeignKeyComparer.Instance.GetHashCode(obj);
    }
}
