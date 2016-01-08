// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ForeignKeyComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        public static readonly ForeignKeyComparer Instance = new ForeignKeyComparer();

        public virtual int Compare(IForeignKey x, IForeignKey y)
        {
            var result = PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
            if (result != 0)
            {
                return result;
            }

            result = PropertyListComparer.Instance.Compare(x.PrincipalKey.Properties, y.PrincipalKey.Properties);
            if (result != 0)
            {
                return result;
            }

            return StringComparer.Ordinal.Compare(x.PrincipalEntityType.Name, y.PrincipalEntityType.Name);
        }

        public virtual bool Equals(IForeignKey x, IForeignKey y)
            => Compare(x, y) == 0;

        public virtual int GetHashCode(IForeignKey obj) =>
            unchecked(
                (((PropertyListComparer.Instance.GetHashCode(obj.PrincipalKey.Properties) * 397)
                  ^ PropertyListComparer.Instance.GetHashCode(obj.Properties)) * 397)
                ^ StringComparer.Ordinal.GetHashCode(obj.PrincipalEntityType.Name));
    }
}
