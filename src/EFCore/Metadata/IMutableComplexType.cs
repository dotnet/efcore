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
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableComplexType : IReadOnlyComplexType, IMutableTypeBase
{
    /// <summary>
    ///     Gets the associated property.
    /// </summary>
    new IMutableComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     Gets or sets the base type of this complex type. Returns <see langword="null" /> if this is not a derived type in an inheritance
    ///     hierarchy.
    /// </summary>
    new IMutableComplexType? BaseType { get; }

    /// <summary>
    ///     Gets the root base type for a given complex type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given complex type is not a derived type, then the same complex type is returned.
    /// </returns>
    new IMutableComplexType GetRootType()
        => (IMutableComplexType)((IReadOnlyTypeBase)this).GetRootType();

    /// <summary>
    ///     Gets all types in the model that derive from this complex type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IMutableComplexType> GetDerivedTypes()
        => ((IReadOnlyTypeBase)this).GetDerivedTypes().Cast<IMutableComplexType>();

    /// <summary>
    ///     Returns all derived types of this complex type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    new IEnumerable<IMutableComplexType> GetDerivedTypesInclusive()
        => ((IReadOnlyTypeBase)this).GetDerivedTypesInclusive().Cast<IMutableComplexType>();

    /// <summary>
    ///     Gets all types in the model that directly derive from this complex type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IMutableComplexType> GetDirectlyDerivedTypes()
        => ((IReadOnlyTypeBase)this).GetDirectlyDerivedTypes().Cast<IMutableComplexType>();
}
