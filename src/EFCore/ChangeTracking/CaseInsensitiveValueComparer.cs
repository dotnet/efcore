// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Case-insensitive value comparison for strings.
/// </summary>
public class CaseInsensitiveValueComparer : ValueComparer<string?>
{
    /// <summary>
    ///     Creates a value comparer instance.
    /// </summary>
    public CaseInsensitiveValueComparer()
        : base(
            (l, r) => string.Equals(l, r, StringComparison.OrdinalIgnoreCase),
            v => v == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(v))
    {
    }
}
