// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Specifies value comparison for arrays where each element pair is compared.
    ///         A new array is constructed when snapshotting.
    ///     </para>
    /// </summary>
    /// <typeparam name="TElement"> The array element type. </typeparam>
    /// <seealso href="https://aka.ms/efcore-docs-value-comparers">Documentation for EF Core value comparers.</seealso>
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
