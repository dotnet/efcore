// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public class ForeignKeyComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        public static readonly ForeignKeyComparer Instance = new ForeignKeyComparer();

        private ForeignKeyComparer()
        {
        }

        public int Compare(IForeignKey x, IForeignKey y)
        {
            return PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
        }

        public bool Equals(IForeignKey x, IForeignKey y)
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
                   && x.PrincipalKey.Equals(y.PrincipalKey)
                   && x.IsUnique == y.IsUnique
                   && x.IsRequired == y.IsRequired;
        }

        public int GetHashCode(IForeignKey obj) => PropertyListComparer.Instance.GetHashCode(obj.Properties);
    }
}
