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
    public class ViewMappingComparer : IEqualityComparer<IViewMapping>, IComparer<IViewMapping>
    {
        private ViewMappingComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly ViewMappingComparer Instance = new ViewMappingComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Compare(IViewMapping x, IViewMapping y)
        {
            var result = EntityTypePathComparer.Instance.Compare(x.EntityType, y.EntityType);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.View.Name, y.View.Name);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.View.Schema, y.View.Schema);
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

            return x.ColumnMappings.Zip(y.ColumnMappings, (xc, yc) => ViewColumnMappingComparer.Instance.Compare(xc, yc))
                .FirstOrDefault(r => r != 0);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Equals(IViewMapping x, IViewMapping y)
            => x.EntityType == y.EntityType
                && x.View == y.View
                && x.IncludesDerivedTypes == y.IncludesDerivedTypes
                && StructuralComparisons.StructuralEqualityComparer.Equals(x.ColumnMappings, y.ColumnMappings);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int GetHashCode(IViewMapping obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.EntityType, EntityTypePathComparer.Instance);
            hashCode.Add(obj.View.Name);
            hashCode.Add(obj.View.Schema);
            foreach (var columnMapping in obj.ColumnMappings)
            {
                hashCode.Add(columnMapping, ViewColumnMappingComparer.Instance);
            }
            hashCode.Add(obj.IncludesDerivedTypes);
            return hashCode.ToHashCode();
        }
    }
}
