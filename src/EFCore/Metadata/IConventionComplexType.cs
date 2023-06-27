// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the type of a complex property of a structural type.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IComplexProperty" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionComplexType : IReadOnlyComplexType, IConventionTypeBase
{
    /// <summary>
    ///     Gets the builder that can be used to configure this property.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the property has been removed from the model.</exception>
    new IConventionComplexTypeBuilder Builder { get; }

    /// <summary>
    ///     Gets the associated property.
    /// </summary>
    new IConventionComplexProperty ComplexProperty { get; }
}
