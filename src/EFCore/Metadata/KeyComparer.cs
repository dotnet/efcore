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

        /// <inheritdoc />
        public int Compare(IKey x, IKey y)
        {
            var result = PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
            return result != 0 ? result : EntityTypeFullNameComparer.Instance.Compare(x.DeclaringEntityType, y.DeclaringEntityType);
        }

        /// <inheritdoc />
        public bool Equals(IKey x, IKey y)
            => Compare(x, y) == 0;

        /// <inheritdoc />
        public int GetHashCode(IKey obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Properties, PropertyListComparer.Instance);
            hashCode.Add(obj.DeclaringEntityType, EntityTypeFullNameComparer.Instance);
            return hashCode.ToHashCode();
        }
    }
}
