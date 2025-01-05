// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a complex property of a structural type.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IComplexProperty" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableComplexProperty : IReadOnlyComplexProperty, IMutablePropertyBase
{
    /// <summary>
    ///     Gets the associated complex type.
    /// </summary>
    new IMutableComplexType ComplexType { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether this property can contain <see langword="null" />.
    /// </summary>
    new bool IsNullable { get; set; }

    /// <inheritdoc />
    bool IReadOnlyComplexProperty.IsNullable
        => IsNullable;
}
