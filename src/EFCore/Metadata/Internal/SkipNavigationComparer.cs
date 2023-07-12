// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class SkipNavigationComparer : IComparer<IReadOnlySkipNavigation>
{
    private SkipNavigationComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SkipNavigationComparer Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(IReadOnlySkipNavigation? x, IReadOnlySkipNavigation? y)
        => (x, y) switch
        {
            (not null, null) => 1,
            (null, not null) => -1,
            (null, null) => 0,
            (not null, not null) => StringComparer.Ordinal.Compare(x.Name, y.Name) is var compare && compare != 0
                ? compare
                : TypeBaseNameComparer.Instance.Compare(x.DeclaringEntityType, y.DeclaringEntityType)
        };
}
