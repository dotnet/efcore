// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the elements of a collection property.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IElementType" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableElementType : IReadOnlyElementType, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the collection property for which this represents the element.
    /// </summary>
    new IMutableProperty CollectionProperty { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether elements of the collection can be <see langword="null" />.
    /// </summary>
    new bool IsNullable { get; set; }

    /// <summary>
    ///     Sets the maximum length of data that is allowed in elements of the collection. For example, if the element type is
    ///     a <see cref="string" /> then this is the maximum number of characters.
    /// </summary>
    /// <param name="maxLength">The maximum length of data that is allowed in this elements of the collection.</param>
    void SetMaxLength(int? maxLength);

    /// <summary>
    ///     Sets the precision of data that is allowed in elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of digits.
    /// </summary>
    /// <param name="precision">The maximum number of digits that is allowed in each element.</param>
    void SetPrecision(int? precision);

    /// <summary>
    ///     Sets the scale of data that is allowed in this elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of decimal places.
    /// </summary>
    /// <param name="scale">The maximum number of decimal places that is allowed in each element.</param>
    void SetScale(int? scale);

    /// <summary>
    ///     Sets a value indicating whether elements of the collection can persist Unicode characters.
    /// </summary>
    /// <param name="unicode">
    ///     <see langword="true" /> if the elements of the collection accept Unicode characters, <see langword="false" /> if they do not,
    ///     or <see langword="null" /> to clear the setting.
    /// </param>
    void SetIsUnicode(bool? unicode);

    /// <summary>
    ///     Sets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <param name="converter">The converter, or <see langword="null" /> to remove any previously set converter.</param>
    void SetValueConverter(ValueConverter? converter);

    /// <summary>
    ///     Sets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <param name="converterType">
    ///     A type that inherits from <see cref="ValueConverter" />, or <see langword="null" /> to remove any previously set converter.
    /// </param>
    void SetValueConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? converterType);

    /// <summary>
    ///     Sets the type that the elements of the collection will be converted to before being sent to the database provider.
    /// </summary>
    /// <param name="providerClrType">The type to use, or <see langword="null" /> to remove any previously set type.</param>
    void SetProviderClrType(Type? providerClrType);

    /// <summary>
    ///     Sets the <see cref="CoreTypeMapping" /> for the given element.
    /// </summary>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> for this element.</param>
    void SetTypeMapping(CoreTypeMapping typeMapping);

    /// <summary>
    ///     Sets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
    void SetValueComparer(ValueComparer? comparer);

    /// <summary>
    ///     Sets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparerType">
    ///     A type that inherits from <see cref="ValueComparer" />, or <see langword="null" /> to remove any previously set comparer.
    /// </param>
    void SetValueComparer([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type? comparerType);

    /// <summary>
    ///     Sets the type of <see cref="JsonValueReaderWriter{TValue}" /> to use for elements of the collection.
    /// </summary>
    /// <param name="readerWriterType">
    ///     A type that inherits from <see cref="JsonValueReaderWriter{TValue}" />, or <see langword="null" /> to use the reader/writer
    ///     from the type mapping.
    /// </param>
    void SetJsonValueReaderWriterType(Type? readerWriterType);

    /// <inheritdoc />
    bool IReadOnlyElementType.IsNullable
        => IsNullable;
}
