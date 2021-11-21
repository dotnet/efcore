// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class ColumnNameComparer : IComparer<string>
{
    private readonly Table _table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ColumnNameComparer(Table table)
    {
        _table = table;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(string? x, string? y)
    {
        var xIndex = -1;
        var yIndex = -1;

        var columns = _table.PrimaryKey?.Columns;
        if (columns != null)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                var name = columns[i].Name;
                if (name == x)
                {
                    xIndex = i;
                }

                if (name == y)
                {
                    yIndex = i;
                }
            }
        }

        if (xIndex == -1
            && yIndex == -1)
        {
            return StringComparer.Ordinal.Compare(x, y);
        }

        if (xIndex > -1
            && yIndex > -1)
        {
            return xIndex - yIndex;
        }

        return xIndex > yIndex
            ? -1
            : 1;
    }
}
