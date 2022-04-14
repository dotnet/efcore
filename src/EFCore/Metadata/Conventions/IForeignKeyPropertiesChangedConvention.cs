// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the foreign key properties or principal key are changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IForeignKeyPropertiesChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the foreign key properties or principal key are changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="oldDependentProperties">The old foreign key properties.</param>
    /// <param name="oldPrincipalKey">The old principal key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessForeignKeyPropertiesChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IReadOnlyList<IConventionProperty> oldDependentProperties,
        IConventionKey oldPrincipalKey,
        IConventionContext<IReadOnlyList<IConventionProperty>> context);
}
