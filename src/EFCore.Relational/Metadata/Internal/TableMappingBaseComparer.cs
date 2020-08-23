// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Sealed for perf
    public sealed class TableMappingBaseComparer : IEqualityComparer<ITableMappingBase>, IComparer<ITableMappingBase>
    {
        private TableMappingBaseComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly TableMappingBaseComparer Instance = new TableMappingBaseComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int Compare(ITableMappingBase x, ITableMappingBase y)
        {
            var result = y.IsSharedTablePrincipal.CompareTo(x.IsSharedTablePrincipal);
            if (result != 0)
            {
                return result;
            }

            result = y.IncludesDerivedTypes.CompareTo(x.IncludesDerivedTypes);
            if (result != 0)
            {
                return result;
            }

            result = y.IsSplitEntityTypePrincipal.CompareTo(x.IsSplitEntityTypePrincipal);
            if (result != 0)
            {
                return result;
            }

            result = EntityTypeFullNameComparer.Instance.Compare(x.EntityType, y.EntityType);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.Table.Name, y.Table.Name);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.Table.Schema, y.Table.Schema);
            if (result != 0)
            {
                return result;
            }

            result = x.ColumnMappings.Count().CompareTo(y.ColumnMappings.Count());
            if (result != 0)
            {
                return result;
            }

            return x.ColumnMappings.Zip(
                    y.ColumnMappings, (xc, yc) =>
                    {
                        var columnResult = StringComparer.Ordinal.Compare(xc.Property.Name, yc.Property.Name);
                        return columnResult != 0 ? columnResult : StringComparer.Ordinal.Compare(xc.Column.Name, yc.Column.Name);
                    })
                .FirstOrDefault(r => r != 0);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals(ITableMappingBase x, ITableMappingBase y)
            => x.EntityType == y.EntityType
                && x.Table == y.Table
                && x.IncludesDerivedTypes == y.IncludesDerivedTypes
                && x.ColumnMappings.SequenceEqual(y.ColumnMappings);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int GetHashCode(ITableMappingBase obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.EntityType, EntityTypeFullNameComparer.Instance);
            hashCode.Add(obj.Table.Name);
            hashCode.Add(obj.Table.Schema);
            foreach (var columnMapping in obj.ColumnMappings)
            {
                hashCode.Add(columnMapping.Property.Name);
                hashCode.Add(columnMapping.Column.Name);
            }

            hashCode.Add(obj.IncludesDerivedTypes);
            return hashCode.ToHashCode();
        }
    }
}
