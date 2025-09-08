// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class ByteArrayComparer : ValueComparer<byte[]>
    {
        public ByteArrayComparer()
            : base(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v == null ? null : v.ToArray())
        {
        }
    }
}
