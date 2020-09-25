// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare
    ///         <see cref="IForeignKey" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public sealed class ForeignKeyComparer : IEqualityComparer<IForeignKey>, IComparer<IForeignKey>
    {
        private ForeignKeyComparer()
        {
        }

        /// <summary>
        ///     The singleton instance of the comparer to use.
        /// </summary>
        public static readonly ForeignKeyComparer Instance = new ForeignKeyComparer();

        /// <inheritdoc />
        public int Compare(IForeignKey x, IForeignKey y)
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

            result = EntityTypeFullNameComparer.Instance.Compare(x.PrincipalEntityType, y.PrincipalEntityType);
            return result != 0 ? result : EntityTypeFullNameComparer.Instance.Compare(x.DeclaringEntityType, y.DeclaringEntityType);
        }

        /// <inheritdoc />
        public bool Equals(IForeignKey x, IForeignKey y)
            => Compare(x, y) == 0;

        /// <inheritdoc />
        public int GetHashCode(IForeignKey obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.PrincipalKey.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.PrincipalEntityType, EntityTypeFullNameComparer.Instance);
            hashCode.Add(obj.DeclaringEntityType, EntityTypeFullNameComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}
