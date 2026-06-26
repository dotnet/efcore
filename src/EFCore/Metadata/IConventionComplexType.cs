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

    /// <summary>
    ///     Gets the base type of this type. Returns <see langword="null" /> if this is not a derived type in an inheritance hierarchy.
    /// </summary>
    new IConventionComplexType? BaseType { get; }

    /// <summary>
    ///     Gets the root base type for a given type.
    /// </summary>
    /// <returns>
    ///     The root base type. If the given type is not a derived type, then the same type is returned.
    /// </returns>
    new IConventionComplexType GetRootType()
        => (IConventionComplexType)((IReadOnlyTypeBase)this).GetRootType();

    /// <summary>
    ///     Gets all types in the model that derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IConventionComplexType> GetDerivedTypes()
        => ((IReadOnlyTypeBase)this).GetDerivedTypes().Cast<IConventionComplexType>();

    /// <summary>
    ///     Returns all derived types of this type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    new IEnumerable<IConventionComplexType> GetDerivedTypesInclusive()
        => ((IReadOnlyTypeBase)this).GetDerivedTypesInclusive().Cast<IConventionComplexType>();

    /// <summary>
    ///     Gets all types in the model that directly derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IConventionComplexType> GetDirectlyDerivedTypes()
        => ((IReadOnlyTypeBase)this).GetDirectlyDerivedTypes().Cast<IConventionComplexType>();
}
