// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ViewColumnMappingComparer : IEqualityComparer<IViewColumnMapping>, IComparer<IViewColumnMapping>
    {
        private ViewColumnMappingComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly ViewColumnMappingComparer Instance = new ViewColumnMappingComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Compare(IViewColumnMapping x, IViewColumnMapping y)
        {
            var result = StringComparer.Ordinal.Compare(x.Property.IsColumnNullable(), y.Property.IsColumnNullable());
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.Property.Name, y.Property.Name);
            if (result != 0)
            {
                return result;
            }

            return StringComparer.Ordinal.Compare(x.Column.Name, y.Column.Name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Equals(IViewColumnMapping x, IViewColumnMapping y)
            => x.Property == y.Property
                && x.Column == y.Column;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int GetHashCode(IViewColumnMapping obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Property.Name);
            hashCode.Add(obj.Column.Name);
            return hashCode.ToHashCode();
        }
    }
}
