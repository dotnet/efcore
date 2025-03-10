// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the type of a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IComplexType : IReadOnlyComplexType, ITypeBase
{
    /// <summary>
    ///     Gets the associated property.
    /// </summary>
    new IComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     Gets the base type of this complex type. Returns <see langword="null" /> if this is not a derived type in an inheritance
    ///     hierarchy.
    /// </summary>
    new IComplexType? BaseType { get; }

    /// <summary>
    ///     Gets all types in the model that derive from this complex type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IComplexType> GetDerivedTypes()
        => ((IReadOnlyTypeBase)this).GetDerivedTypes().Cast<IComplexType>();

    /// <summary>
    ///     Returns all derived types of this complex type, including the type itself.
    /// </summary>
    /// <returns>Derived types.</returns>
    new IEnumerable<IComplexType> GetDerivedTypesInclusive()
        => ((IReadOnlyTypeBase)this).GetDerivedTypesInclusive().Cast<IComplexType>();

    /// <summary>
    ///     Gets all types in the model that directly derive from this complex type.
    /// </summary>
    /// <returns>The derived types.</returns>
    new IEnumerable<IComplexType> GetDirectlyDerivedTypes();

    /// <summary>
    ///     Gets the entity type on which the complex property chain is declared.
    /// </summary>
    IEntityType ITypeBase.ContainingEntityType
        => (IEntityType)((IReadOnlyComplexType)this).ContainingEntityType;

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetPropertiesInHierarchy()
        => GetDeclaredProperties();

    /// <summary>
    ///     Gets all properties declared on the base types and types derived from this entity type, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetFlattenedPropertiesInHierarchy()
        => GetFlattenedDeclaredProperties();
}
