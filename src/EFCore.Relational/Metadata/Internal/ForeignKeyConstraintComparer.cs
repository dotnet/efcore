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
    // Sealed for perf
    public sealed class ForeignKeyConstraintComparer : IEqualityComparer<IForeignKeyConstraint>, IComparer<IForeignKeyConstraint>
    {
        private ForeignKeyConstraintComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly ForeignKeyConstraintComparer Instance = new ForeignKeyConstraintComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int Compare(IForeignKeyConstraint x, IForeignKeyConstraint y)
        {
            var result = StringComparer.Ordinal.Compare(x.Name, y.Name);
            if (result != 0)
            {
                return result;
            }

            result = ColumnListComparer.Instance.Compare(x.Columns, y.Columns);
            if (result != 0)
            {
                return result;
            }

            result = ColumnListComparer.Instance.Compare(x.PrincipalColumns, y.PrincipalColumns);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.PrincipalTable.Name, y.PrincipalTable.Name);
            return result != 0 ? result : StringComparer.Ordinal.Compare(x.Table.Name, y.Table.Name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals(IForeignKeyConstraint x, IForeignKeyConstraint y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int GetHashCode(IForeignKeyConstraint obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Columns, ColumnListComparer.Instance);
            hashCode.Add(obj.PrincipalColumns, ColumnListComparer.Instance);
            hashCode.Add(obj.Table.Name);
            hashCode.Add(obj.PrincipalTable.Name);
            return hashCode.ToHashCode();
        }
    }
}
