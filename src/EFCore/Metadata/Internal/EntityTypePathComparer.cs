// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityTypePathComparer : IComparer<IEntityType>
    {
        private EntityTypePathComparer()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly EntityTypePathComparer Instance = new EntityTypePathComparer();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int Compare(IEntityType x, IEntityType y)
        {
            var result = StringComparer.Ordinal.Compare(x.Name, y.Name);
            if (result != 0)
            {
                return result;
            }

            while (true)
            {
                var xDefiningNavigationName = x.DefiningNavigationName;
                var yDefiningNavigationName = y.DefiningNavigationName;

                if (xDefiningNavigationName == null
                    && yDefiningNavigationName == null)
                {
                    return StringComparer.Ordinal.Compare(x.Name, y.Name);
                }

                if (xDefiningNavigationName == null)
                {
                    return -1;
                }

                if (yDefiningNavigationName == null)
                {
                    return 1;
                }

                result = StringComparer.Ordinal.Compare(xDefiningNavigationName, yDefiningNavigationName);
                if (result != 0)
                {
                    return result;
                }

                x = x.DefiningEntityType;
                y = y.DefiningEntityType;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int GetHashCode([NotNull] IEntityType entityType)
        {
            var result = 0;
            while (true)
            {
                result = (result * 397)
                         ^ StringComparer.Ordinal.GetHashCode(entityType.Name);
                var definingNavigationName = entityType.DefiningNavigationName;
                if (definingNavigationName == null)
                {
                    return result;
                }

                result = (result * 397)
                         ^ StringComparer.Ordinal.GetHashCode(definingNavigationName);
                entityType = entityType.DefiningEntityType;
            }
        }
    }
}
