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
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionElementType : IReadOnlyElementType, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the collection property for which this represents the element.
    /// </summary>
    new IConventionProperty CollectionProperty { get; }

    /// <summary>
    ///     Returns the configuration source for this element.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Gets the builder that can be used to configure this element.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the element has been removed from the model.</exception>
    new IConventionElementTypeBuilder Builder { get; }

    /// <summary>
    ///     Sets a value indicating whether elements in the collection can be <see langword="null" />.
    /// </summary>
    /// <param name="nullable">
    ///     A value indicating whether whether elements in the collection can be <see langword="null" />, or <see langword="null" /> to
    ///     reset to the default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsNullable(bool? nullable, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.IsNullable" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.IsNullable" />.</returns>
    ConfigurationSource? GetIsNullableConfigurationSource();

    /// <summary>
    ///     Sets the <see cref="CoreTypeMapping" /> for the given element.
    /// </summary>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> for this element.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    CoreTypeMapping? SetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="CoreTypeMapping" /> of the element.
    /// </summary>
    /// <returns>The <see cref="ConfigurationSource" /> for <see cref="CoreTypeMapping" /> of the element.</returns>
    ConfigurationSource? GetTypeMappingConfigurationSource();

    /// <summary>
    ///     Sets the maximum length of data that is allowed in elements of the collection. For example, if the element type is
    ///     a <see cref="string" /> then this is the maximum number of characters.
    /// </summary>
    /// <param name="maxLength">The maximum length of data that is allowed in each element.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured property.</returns>
    int? SetMaxLength(int? maxLength, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetMaxLength" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetMaxLength" />.</returns>
    ConfigurationSource? GetMaxLengthConfigurationSource();

    /// <summary>
    ///     Sets the precision of data that is allowed in elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of digits.
    /// </summary>
    /// <param name="precision">The maximum number of digits that is allowed in each element.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    int? SetPrecision(int? precision, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetPrecision" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetPrecision" />.</returns>
    ConfigurationSource? GetPrecisionConfigurationSource();

    /// <summary>
    ///     Sets the scale of data that is allowed in this elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of decimal places.
    /// </summary>
    /// <param name="scale">The maximum number of decimal places that is allowed in each element.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    int? SetScale(int? scale, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetScale" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetScale" />.</returns>
    ConfigurationSource? GetScaleConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether elements of the collection can persist Unicode characters.
    /// </summary>
    /// <param name="unicode">
    ///     <see langword="true" /> if the elements of the collection accept Unicode characters, <see langword="false" /> if they do not,
    ///     or <see langword="null" /> to clear the setting.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsUnicode(bool? unicode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.IsUnicode" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.IsUnicode" />.</returns>
    ConfigurationSource? GetIsUnicodeConfigurationSource();

    /// <summary>
    ///     Sets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <param name="converter">The converter, or <see langword="null" /> to remove any previously set converter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    ValueConverter? SetValueConverter(ValueConverter? converter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <param name="converterType">
    ///     A type that inherits from <see cref="ValueConverter" />, or <see langword="null" /> to remove any previously set converter.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    Type? SetValueConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetValueConverter" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetValueConverter" />.</returns>
    ConfigurationSource? GetValueConverterConfigurationSource();

    /// <summary>
    ///     Sets the type that the elements of the collection will be converted to before being sent to the database provider.
    /// </summary>
    /// <param name="providerClrType">The type to use, or <see langword="null" /> to remove any previously set type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    Type? SetProviderClrType(Type? providerClrType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetProviderClrType" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetProviderClrType" />.</returns>
    ConfigurationSource? GetProviderClrTypeConfigurationSource();

    /// <summary>
    ///     Sets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    ValueComparer? SetValueComparer(ValueComparer? comparer, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparerType">
    ///     A type that inherits from <see cref="ValueComparer" />, or <see langword="null" /> to remove any previously set comparer.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type? SetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetValueComparer" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetValueComparer" />.</returns>
    ConfigurationSource? GetValueComparerConfigurationSource();

    /// <summary>
    ///     Sets the type of <see cref="JsonValueReaderWriter{TValue}" /> to use for elements of the collection.
    /// </summary>
    /// <param name="readerWriterType">
    ///     A type that inherits from <see cref="JsonValueReaderWriter{TValue}" />, or <see langword="null" /> to use the reader/writer
    ///     from the type mapping.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    Type? SetJsonValueReaderWriterType(Type? readerWriterType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.GetJsonValueReaderWriter" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.GetJsonValueReaderWriter" />.</returns>
    ConfigurationSource? GetJsonValueReaderWriterTypeConfigurationSource();
}
