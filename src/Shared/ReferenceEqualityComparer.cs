// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Internal;

internal sealed class LegacyReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
{
    private LegacyReferenceEqualityComparer()
    {
    }

    public static LegacyReferenceEqualityComparer Instance { get; } = new();

    public new bool Equals(object? x, object? y)
        => ReferenceEquals(x, y);

    public int GetHashCode(object obj)
        => RuntimeHelpers.GetHashCode(obj);

    bool IEqualityComparer<object>.Equals(object? x, object? y)
        => ReferenceEquals(x, y);

    int IEqualityComparer.GetHashCode(object obj)
        => RuntimeHelpers.GetHashCode(obj);

    bool IEqualityComparer.Equals(object? x, object? y)
        => ReferenceEquals(x, y);

    int IEqualityComparer<object>.GetHashCode(object obj)
        => RuntimeHelpers.GetHashCode(obj);
}
