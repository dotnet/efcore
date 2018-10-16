// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Specifies value snapshotting and comparison for arrays where each element is compared
    ///         a new array is constructed when snapshotting.
    ///     </para>
    /// </summary>
    /// <typeparam name="TElement"> The array element type. </typeparam>
    public class ArrayStructuralComparer<TElement> : ValueComparer<TElement[]>
    {
        /// <summary>
        ///     Creates a comparer instance.
        /// </summary>
        public ArrayStructuralComparer()
            : base(
                CreateDefaultEqualsExpression(),
                CreateDefaultHashCodeExpression(favorStructuralComparisons: true),
                v => v == null ? null : v.ToArray())
        {
        }
    }
}
