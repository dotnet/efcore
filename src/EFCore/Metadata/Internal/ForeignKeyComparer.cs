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
    public class ForeignKeyComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        private ForeignKeyComparer()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly ForeignKeyComparer Instance = new ForeignKeyComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Compare(IForeignKey x, IForeignKey y)
        {
            var result = PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
            if (result != 0)
            {
                return result;
            }

            result = PropertyListComparer.Instance.Compare(x.PrincipalKey.Properties, y.PrincipalKey.Properties);
            if (result != 0)
            {
                return result;
            }

            result = EntityTypePathComparer.Instance.Compare(x.PrincipalEntityType, y.PrincipalEntityType);
            return result != 0 ? result : EntityTypePathComparer.Instance.Compare(x.DeclaringEntityType, y.DeclaringEntityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Equals(IForeignKey x, IForeignKey y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int GetHashCode(IForeignKey obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.PrincipalKey.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.PrincipalEntityType, EntityTypePathComparer.Instance);
            hashCode.Add(obj.DeclaringEntityType, EntityTypePathComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}
