// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the uniqueness for a foreign key is changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IForeignKeyUniquenessChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the uniqueness for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessForeignKeyUniquenessChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context);
}
