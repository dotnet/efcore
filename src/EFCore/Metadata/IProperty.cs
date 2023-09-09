// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a scalar property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IProperty : IReadOnlyProperty, IPropertyBase
{
    /// <summary>
    ///     Gets the entity type that this property belongs to.
    /// </summary>
    [Obsolete("Use DeclaringType and cast to IEntityType or IComplexType")]
    new IEntityType DeclaringEntityType
        => (IEntityType)DeclaringType;

    /// <summary>
    ///     Creates an <see cref="IEqualityComparer{T}" /> for values of the given property type.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <returns>A new equality comparer.</returns>
    IEqualityComparer<TProperty> CreateKeyEqualityComparer<TProperty>()
        => NullableComparerAdapter<TProperty>.Wrap(GetKeyValueComparer());

    /// <summary>
    ///     Finds the first principal property that the given property is constrained by
    ///     if the given property is part of a foreign key.
    /// </summary>
    /// <returns>The first associated principal property, or <see langword="null" /> if none exists.</returns>
    new IProperty? FindFirstPrincipal()
        => (IProperty?)((IReadOnlyProperty)this).FindFirstPrincipal();

    /// <summary>
    ///     Finds the list of principal properties including the given property that the given property is constrained by
    ///     if the given property is part of a foreign key.
    /// </summary>
    /// <returns>The list of all associated principal properties including the given property.</returns>
    new IReadOnlyList<IProperty> GetPrincipals()
        => GetPrincipals<IProperty>();

    /// <summary>
    ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
    ///     is included).
    /// </summary>
    /// <returns>
    ///     The foreign keys that use this property.
    /// </returns>
    new IEnumerable<IForeignKey> GetContainingForeignKeys();

    /// <summary>
    ///     Gets all indexes that use this property (including composite indexes in which this property
    ///     is included).
    /// </summary>
    /// <returns>
    ///     The indexes that use this property.
    /// </returns>
    new IEnumerable<IIndex> GetContainingIndexes();

    /// <summary>
    ///     Gets the primary key that uses this property (including a composite primary key in which this property
    ///     is included).
    /// </summary>
    /// <returns>
    ///     The primary that use this property, or <see langword="null" /> if it is not part of the primary key.
    /// </returns>
    new IKey? FindContainingPrimaryKey()
        => (IKey?)((IReadOnlyProperty)this).FindContainingPrimaryKey();

    /// <summary>
    ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
    ///     is included).
    /// </summary>
    /// <returns>
    ///     The primary and alternate keys that use this property.
    /// </returns>
    new IEnumerable<IKey> GetContainingKeys();

    /// <summary>
    ///     Gets the <see cref="ValueComparer" /> for this property.
    /// </summary>
    /// <returns>The comparer.</returns>
    new ValueComparer GetValueComparer();

    /// <summary>
    ///     Gets the <see cref="ValueComparer" /> to use with keys for this property.
    /// </summary>
    /// <returns>The comparer.</returns>
    new ValueComparer GetKeyValueComparer();

    /// <summary>
    ///     Gets the <see cref="ValueComparer" /> to use for the provider values for this property.
    /// </summary>
    /// <returns>The comparer.</returns>
    new ValueComparer GetProviderValueComparer();

    /// <summary>
    ///     Gets the configuration for elements of the primitive collection represented by this property.
    /// </summary>
    /// <returns>The configuration for the elements.</returns>
    new IElementType? GetElementType();

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessedMemberTypes =
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces;
}
