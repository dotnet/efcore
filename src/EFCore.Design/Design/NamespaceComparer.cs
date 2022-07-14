// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     A custom string comparer to sort using statements to have System prefixed namespaces first.
/// </summary>
public class NamespaceComparer : IComparer<string>
{
    /// <inheritdoc />
    public virtual int Compare(string? x, string? y)
    {
        var xSystemNamespace = x != null && (x == "System" || x.StartsWith("System.", StringComparison.Ordinal));
        var ySystemNamespace = y != null && (y == "System" || y.StartsWith("System.", StringComparison.Ordinal));

        return xSystemNamespace && !ySystemNamespace
            ? -1
            : !xSystemNamespace && ySystemNamespace
                ? 1
                : string.CompareOrdinal(x, y);
    }
}
