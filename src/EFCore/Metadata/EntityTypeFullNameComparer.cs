// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare
    ///         <see cref="IEntityType" /> instances by name including the defining entity type when present.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public sealed class EntityTypeFullNameComparer : IComparer<IEntityType>, IEqualityComparer<IEntityType>
    {
        private EntityTypeFullNameComparer()
        {
        }

        /// <summary>
        ///     The singleton instance of the comparer to use.
        /// </summary>
        public static readonly EntityTypeFullNameComparer Instance = new EntityTypeFullNameComparer();

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise. </returns>
        public int Compare(IEntityType x, IEntityType y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

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
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />. </returns>
        public bool Equals(IEntityType x, IEntityType y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        public int GetHashCode(IEntityType obj)
        {
            var hash = new HashCode();
            while (true)
            {
                hash.Add(obj.Name, StringComparer.Ordinal);
                var definingNavigationName = obj.DefiningNavigationName;
                if (definingNavigationName == null)
                {
                    return hash.ToHashCode();
                }

                hash.Add(definingNavigationName, StringComparer.Ordinal);
                obj = obj.DefiningEntityType;
            }
        }
    }
}
