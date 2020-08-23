// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare current values
    ///         contained in <see cref="IUpdateEntry" /> internal tracking entities.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TProperty"> The type of the property. </typeparam>
    public sealed class EntryCurrentValueComparer<TProperty> : IComparer<IUpdateEntry>, IEqualityComparer<IUpdateEntry>
    {
        private readonly IPropertyBase _property;
        private readonly IComparer<TProperty> _underlyingComparer;

        /// <summary>
        ///     Creates a new <see cref="EntryCurrentValueComparer" /> instance using a the default comparer for the property type.
        /// </summary>
        /// <param name="property"> The property to use for comparisons. </param>
        public EntryCurrentValueComparer([NotNull] IPropertyBase property)
        {
            _property = property;
            _underlyingComparer = Comparer<TProperty>.Default;
        }

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise. </returns>
        public int Compare(IUpdateEntry x, IUpdateEntry y)
            => _underlyingComparer.Compare(
                x.GetCurrentValue<TProperty>(_property),
                y.GetCurrentValue<TProperty>(_property));

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />. </returns>
        public bool Equals(IUpdateEntry x, IUpdateEntry y)
            => Compare(x, y) == 0;

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj"> The for which a hash code is to be returned. </param>
        /// <returns> A hash code for the specified object. </returns>
        public int GetHashCode(IUpdateEntry obj)
            => obj.GetCurrentValue<TProperty>(_property).GetHashCode();
    }
}
