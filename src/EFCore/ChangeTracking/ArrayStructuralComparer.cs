// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Specifies value comparison for arrays where each element pair is compared.
///     A new array is constructed when snapshotting.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
/// </remarks>
/// <typeparam name="TElement">The array element type.</typeparam>
public class ArrayStructuralComparer<TElement> : ValueComparer<TElement[]>
{
    /// <summary>
    ///     Creates a comparer instance.
    /// </summary>
    public ArrayStructuralComparer()
        : base(
            CreateDefaultEqualsExpression(),
            CreateDefaultHashCodeExpression(favorStructuralComparisons: true),
            v => v.ToArray())
    {
    }
}
