// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IEqualityComparer" /> and <see cref="IEqualityComparer{T}" /> to compare
    ///         objects by reference.
    ///     </para>
    ///     <para>
    ///         This comparer is used internally by EF when creating <see cref="HashSet{T}" /> collection navigations.
    ///         It is advised that manually created collection navigations do likewise, or otherwise ensure that
    ///         every entity instance compares different to another instance.
    ///     </para>
    /// </summary>
    public sealed class ReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
    {
        private ReferenceEqualityComparer()
        {
        }

        /// <summary>
        ///     The singleton instance of the comparer to use.
        /// </summary>
        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> True if the specified objects are equal; otherwise, false. </returns>
        public new bool Equals([CanBeNull] object x, [CanBeNull] object y)
            => ReferenceEquals(x, y);

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        public int GetHashCode([NotNull] object obj)
            => RuntimeHelpers.GetHashCode(obj);

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> True if the specified objects are equal; otherwise, false. </returns>
        bool IEqualityComparer<object>.Equals(object x, object y)
            => ReferenceEquals(x, y);

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        int IEqualityComparer.GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> True if the specified objects are equal; otherwise, false. </returns>
        bool IEqualityComparer.Equals(object x, object y)
            => ReferenceEquals(x, y);

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        int IEqualityComparer<object>.GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);
    }
}
