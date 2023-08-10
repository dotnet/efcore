// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the elements of a collection property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyElementType : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the collection property for which this represents the element.
    /// </summary>
    IReadOnlyProperty CollectionProperty { get; }

    /// <summary>
    ///     The type of elements in the collection.
    /// </summary>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes | IProperty.DynamicallyAccessedMemberTypes)]
    Type ClrType { get; }

    /// <summary>
    ///     Gets a value indicating whether elements of the collection can be <see langword="null" />.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    ///     Returns the <see cref="CoreTypeMapping" /> for the elements of the collection from a finalized model.
    /// </summary>
    /// <returns>The type mapping.</returns>
    CoreTypeMapping GetTypeMapping()
    {
        var mapping = FindTypeMapping();
        if (mapping == null)
        {
            throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetTypeMapping)));
        }

        return mapping;
    }

    /// <summary>
    ///     Returns the type mapping for elements of the collection.
    /// </summary>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    CoreTypeMapping? FindTypeMapping();

    /// <summary>
    ///     Gets the maximum length of data that is allowed in elements of the collection. For example, if the element type is
    ///     a <see cref="string" /> then this is the maximum number of characters.
    /// </summary>
    /// <returns>
    ///     The maximum length, <c>-1</c> if the property has no maximum length, or <see langword="null" /> if the maximum length hasn't been
    ///     set.
    /// </returns>
    int? GetMaxLength();

    /// <summary>
    ///     Gets the precision of data that is allowed in elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of digits.
    /// </summary>
    /// <returns>The precision, or <see langword="null" /> if none is defined.</returns>
    int? GetPrecision();

    /// <summary>
    ///     Gets the scale of data that is allowed in this elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of decimal places.
    /// </summary>
    /// <returns>The scale, or <see langword="null" /> if none is defined.</returns>
    int? GetScale();

    /// <summary>
    ///     Gets a value indicating whether elements of the collection can persist Unicode characters.
    /// </summary>
    /// <returns>The Unicode setting, or <see langword="null" /> if none is defined.</returns>
    bool? IsUnicode();

    /// <summary>
    ///     Gets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <returns>The converter, or <see langword="null" /> if none has been set.</returns>
    ValueConverter? GetValueConverter();

    /// <summary>
    ///     Gets the type that the elements of the collection will be converted to before being sent to the database provider.
    /// </summary>
    /// <returns>The provider type, or <see langword="null" /> if none has been set.</returns>
    Type? GetProviderClrType();

    /// <summary>
    ///     Gets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <returns>The comparer, or <see langword="null" /> if none has been set.</returns>
    ValueComparer? GetValueComparer();

    /// <summary>
    ///     Gets the type of <see cref="JsonValueReaderWriter{TValue}" /> to use for elements of the collection.
    /// </summary>
    /// <returns>The reader/writer, or <see langword="null" /> if none has been set.</returns>
    JsonValueReaderWriter? GetJsonValueReaderWriter();

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("Element type: ");
            }

            builder.Append(ClrType.ShortDisplayName());

            if (!IsNullable)
            {
                builder.Append(" Required");
            }

            if (GetMaxLength() != null)
            {
                builder.Append(" MaxLength(").Append(GetMaxLength()).Append(')');
            }

            if (IsUnicode() == false)
            {
                builder.Append(" ANSI");
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
