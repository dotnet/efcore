// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Associates a <see cref="RelationalTypeMapping" /> with an optional <see cref="IProperty" />
///     and an index into the data reader for use when reading and converting values from the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class TypeMaterializationInfo
{
    /// <summary>
    ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
    /// </summary>
    /// <param name="modelClrType">The type that is needed in the model after conversion.</param>
    /// <param name="property">The property associated with the type, or <see langword="null" /> if none.</param>
    /// <param name="mapping">The type mapping to use or <see langword="null" /> to infer one.</param>
    /// <param name="nullable">A value indicating whether the value could be null.</param>
    public TypeMaterializationInfo(
        Type modelClrType,
        IProperty? property,
        RelationalTypeMapping mapping,
        bool? nullable = null)
    {
        ProviderClrType = mapping.Converter?.ProviderClrType ?? modelClrType;
        ModelClrType = modelClrType;
        Mapping = mapping;
        Property = property;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The type that will be read from the database provider.
    /// </summary>
    public virtual Type ProviderClrType { get; }

    /// <summary>
    ///     The type that is needed in the model after conversion.
    /// </summary>
    public virtual Type ModelClrType { get; }

    /// <summary>
    ///     The type mapping for the value to be read.
    /// </summary>
    public virtual RelationalTypeMapping Mapping { get; }

    /// <summary>
    ///     The property associated with the type, or <see langword="null" /> if none.
    /// </summary>
    public virtual IProperty? Property { get; }

    /// <summary>
    ///     Whether or not the value can be null.
    /// </summary>
    public virtual bool? IsNullable { get; }

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    protected virtual bool Equals(TypeMaterializationInfo other)
        => ProviderClrType == other.ProviderClrType
            && ModelClrType == other.ModelClrType
            && Equals(Mapping, other.Mapping)
            && Equals(Property, other.Property)
            && IsNullable == other.IsNullable;

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj)
        => obj is not null
            && (ReferenceEquals(this, obj)
                || obj.GetType() == GetType()
                && Equals((TypeMaterializationInfo)obj));

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
        => HashCode.Combine(ProviderClrType, ModelClrType, Mapping, Property, IsNullable);
}
