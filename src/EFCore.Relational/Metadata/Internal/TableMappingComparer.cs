// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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
    public class TableMappingComparer : IEqualityComparer<ITableMapping>, IComparer<ITableMapping>
    {
        private TableMappingComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly TableMappingComparer Instance = new TableMappingComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Compare(ITableMapping x, ITableMapping y)
        {
            var result = EntityTypePathComparer.Instance.Compare(x.EntityType, y.EntityType);
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

            result = x.IncludesDerivedTypes.CompareTo(y.IncludesDerivedTypes);
            if (result != 0)
            {
                return result;
            }

            result = x.ColumnMappings.Count().CompareTo(y.ColumnMappings.Count());
            if (result != 0)
            {
                return result;
            }

            return x.ColumnMappings.Zip(y.ColumnMappings, (xc, yc) => ColumnMappingComparer.Instance.Compare(xc, yc))
                .FirstOrDefault(r => r != 0);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Equals(ITableMapping x, ITableMapping y)
            => x.EntityType == y.EntityType
                && x.Table == y.Table
                && x.IncludesDerivedTypes == y.IncludesDerivedTypes
                && StructuralComparisons.StructuralEqualityComparer.Equals(x.ColumnMappings, y.ColumnMappings);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int GetHashCode(ITableMapping obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.EntityType, EntityTypePathComparer.Instance);
            hashCode.Add(obj.Table.Name);
            hashCode.Add(obj.Table.Schema);
            foreach (var columnMapping in obj.ColumnMappings)
            {
                hashCode.Add(columnMapping, ColumnMappingComparer.Instance);
            }
            hashCode.Add(obj.IncludesDerivedTypes);
            return hashCode.ToHashCode();
        }
    }
}
