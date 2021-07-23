// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare
    ///         <see cref="IReadOnlyKey" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public sealed class KeyComparer : IEqualityComparer<IReadOnlyKey>, IComparer<IReadOnlyKey>
    {
        private KeyComparer()
        {
        }

        /// <summary>
        ///     The singleton instance of the comparer to use.
        /// </summary>
        public static readonly KeyComparer Instance = new();

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise. </returns>
        public int Compare(IReadOnlyKey? x, IReadOnlyKey? y)
        {
            var result = PropertyListComparer.Instance.Compare(x?.Properties, y?.Properties);
            return result != 0 ? result : EntityTypeFullNameComparer.Instance.Compare(x?.DeclaringEntityType, y?.DeclaringEntityType);
        }

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />. </returns>
        public bool Equals(IReadOnlyKey? x, IReadOnlyKey? y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        public int GetHashCode(IReadOnlyKey obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.DeclaringEntityType, EntityTypeFullNameComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}
