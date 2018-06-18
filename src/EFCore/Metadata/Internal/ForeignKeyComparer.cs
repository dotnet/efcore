// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKeyComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        private ForeignKeyComparer()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly ForeignKeyComparer Instance = new ForeignKeyComparer();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Equals(IForeignKey x, IForeignKey y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int GetHashCode(IForeignKey obj) =>
            unchecked(
                ((((PropertyListComparer.Instance.GetHashCode(obj.PrincipalKey.Properties) * 397)
                   ^ PropertyListComparer.Instance.GetHashCode(obj.Properties)) * 397)
                 ^ EntityTypePathComparer.Instance.GetHashCode(obj.PrincipalEntityType)) * 397)
            ^ EntityTypePathComparer.Instance.GetHashCode(obj.DeclaringEntityType);
    }
}
