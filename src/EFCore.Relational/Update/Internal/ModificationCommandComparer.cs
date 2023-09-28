// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ModificationCommandComparer : IComparer<IReadOnlyModificationCommand>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int Compare(IReadOnlyModificationCommand? x, IReadOnlyModificationCommand? y)
    {
        var result = 0;
        if (ReferenceEquals(x, y))
        {
            return result;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        result = StringComparer.Ordinal.Compare(x.TableName, y.TableName);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Schema, y.Schema);
        if (result != 0)
        {
            return result;
        }

        var xState = x.EntityState;
        result = (int)xState - (int)y.EntityState;
        if (result != 0)
        {
            return result;
        }

        if (x.Entries.Count > 0
            && y.Entries.Count > 0)
        {
            var xEntry = x.Entries[0];
            var yEntry = y.Entries[0];

            var xEntityType = xEntry.EntityType;
            var yEntityType = yEntry.EntityType;
            if (xEntityType != yEntityType)
            {
                result = StringComparer.Ordinal.Compare(xEntityType.Name, yEntityType.Name);
                if (result != 0)
                {
                    return result;
                }
            }

            var xKey = xEntry.EntityType.FindPrimaryKey()!;
            for (var i = 0; i < xKey.Properties.Count; i++)
            {
                var xKeyProperty = xKey.Properties[i];

                result = xKeyProperty.GetCurrentValueComparer().Compare(xEntry, yEntry);
                if (result != 0)
                {
                    return result;
                }
            }
        }
        else
        {
            var xKey = x.Table?.PrimaryKey;
            var yKey = y.Table?.PrimaryKey;
            if (xKey != null && yKey != null)
            {
                for (var i = 0; i < xKey.Columns.Count; i++)
                {
                    var xKeyColumn = xKey.Columns[i];

                    if (x.ColumnModifications.First(m => m.ColumnName == xKeyColumn.Name).Value is not IComparable xModification
                        || y.ColumnModifications.First(m => m.ColumnName == xKeyColumn.Name).Value is not IComparable yModification)
                    {
                        continue;
                    }

                    result = xModification.CompareTo(yModification);
                    if (result != 0)
                    {
                        return result;
                    }
                }
            }
        }

        return result;
    }
}
