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

        /// <inheritdoc />
        public int Compare(IUpdateEntry x, IUpdateEntry y)
            => _underlyingComparer.Compare(
                x.GetCurrentValue<TProperty>(_property),
                y.GetCurrentValue<TProperty>(_property));

        /// <inheritdoc />
        public bool Equals(IUpdateEntry x, IUpdateEntry y)
            => Compare(x, y) == 0;

        /// <inheritdoc />
        public int GetHashCode(IUpdateEntry obj)
            => obj.GetCurrentValue<TProperty>(_property).GetHashCode();
    }
}
