// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the sort order of an index is changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IIndexSortOrderChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the uniqueness for an index is changed.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessIndexSortOrderChanged(
        IConventionIndexBuilder indexBuilder,
        IConventionContext<IReadOnlyList<bool>?> context);
}
