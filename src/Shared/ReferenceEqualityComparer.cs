// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Internal
{
    // Sealed for perf
    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
    {
        private ReferenceEqualityComparer()
        {
        }

        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

        bool IEqualityComparer.Equals(object x, object y) => ReferenceEquals(x, y);

        int IEqualityComparer.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);

        int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
