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
    ///         <see cref="IKey" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public sealed class KeyComparer : IEqualityComparer<IKey>, IComparer<IKey>
    {
        private KeyComparer()
        {
        }

        /// <summary>
        ///     The singleton instance of the comparer to use.
        /// </summary>
        public static readonly KeyComparer Instance = new KeyComparer();

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise. </returns>
        public int Compare(IKey x, IKey y)
        {
            var result = PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
            return result != 0 ? result : EntityTypeFullNameComparer.Instance.Compare(x.DeclaringEntityType, y.DeclaringEntityType);
        }

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />. </returns>
        public bool Equals(IKey x, IKey y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        public int GetHashCode(IKey obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.DeclaringEntityType, EntityTypeFullNameComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}
