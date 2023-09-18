// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionElementType" /> for a primitive collection
///         from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionElementTypeBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     Gets the element type being configured.
    /// </summary>
    new IConventionElementType Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionElementTypeBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionElementTypeBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionElementTypeBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures whether elements of the collection must have a value or can be <see langword="null" />.
    ///     An element can only be configured as non-required if it is based on a CLR type that can be
    ///     assigned <see langword="null" />.
    /// </summary>
    /// <param name="required">A value indicating whether elements of the collection must not be <see langword="null" />.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the requiredness was configured,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? IsRequired(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this element requiredness can be configured from the current configuration source.
    /// </summary>
    /// <param name="required">
    ///     A value indicating whether the elements are required, or <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the element requiredness can be configured.</returns>
    bool CanSetIsRequired(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the maximum length of data that can be stored in elements of the collection.
    /// </summary>
    /// <param name="maxLength">
    ///     The maximum length of data allowed in elements of the collection. A value of <c>-1</c> indicates that elements of the
    ///     collection have no maximum length.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasMaxLength(int? maxLength, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the maximum length of elements can be set from the current configuration source.
    /// </summary>
    /// <param name="maxLength">The maximum length of elements in the collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the maximum length of data allowed can be set for the elements.</returns>
    bool CanSetMaxLength(int? maxLength, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures whether elements of the collection are capable of persisting unicode characters.
    /// </summary>
    /// <param name="unicode">A value indicating whether elements of the collection can contain unicode characters.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? IsUnicode(bool? unicode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the elements can be configured as capable of persisting unicode characters
    ///     from the current configuration source.
    /// </summary>
    /// <param name="unicode">A value indicating whether the elements can contain unicode characters.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the capability of persisting unicode characters can be configured.</returns>
    bool CanSetIsUnicode(bool? unicode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the precision of elements of the collection.
    /// </summary>
    /// <param name="precision">The precision of elements of the collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasPrecision(int? precision, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the precision of elements can be set from the current configuration source.
    /// </summary>
    /// <param name="precision">The precision of the elements.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the precision of data allowed can be set.</returns>
    bool CanSetPrecision(int? precision, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the scale of elements of the collection.
    /// </summary>
    /// <param name="scale">The scale of elements of the collection.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasScale(int? scale, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the scale of elements can be set from the current configuration source.
    /// </summary>
    /// <param name="scale">The scale of the elements.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the scale of data allowed can be set.</returns>
    bool CanSetScale(int? scale, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures elements of the collection so their values are converted before writing to the database and converted back
    ///     when reading from the database.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasConversion(ValueConverter? converter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the <see cref="ValueConverter" /> can be configured for the elements
    ///     from the current configuration source.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the <see cref="ValueConverter" /> can be configured.
    /// </returns>
    bool CanSetConversion(ValueConverter? converter, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures elements of the collection so their values are converted before writing to the database and converted back
    ///     when reading from the database.
    /// </summary>
    /// <param name="providerClrType">The type to convert to and from.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasConversion(Type? providerClrType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given type to convert values to and from
    ///     can be configured for the elements from the current configuration source.
    /// </summary>
    /// <param name="providerClrType">The type to convert to and from.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the given type to convert values to and from can be configured.
    /// </returns>
    bool CanSetConversion(Type? providerClrType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures elements of the collection so their values are converted before writing to the database and converted back
    ///     when reading from the database.
    /// </summary>
    /// <param name="converterType">
    ///     A type that derives from <see cref="ValueConverter" />,
    ///     or <see langword="null" /> to remove any previously set converter.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, or <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the <see cref="ValueConverter" /> can be configured for the elements
    ///     from the current configuration source.
    /// </summary>
    /// <param name="converterType">
    ///     A type that derives from <see cref="ValueConverter" />,
    ///     or <see langword="null" /> to remove any previously set converter.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the <see cref="ValueConverter" /> can be configured.
    /// </returns>
    bool CanSetConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="CoreTypeMapping" /> for elements of the collection.
    /// </summary>
    /// <param name="typeMapping">The type mapping, or <see langword="null" /> to remove any previously set type mapping.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, or <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasTypeMapping(CoreTypeMapping? typeMapping, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given <see cref="CoreTypeMapping" />
    ///     can be configured from the current configuration source.
    /// </summary>
    /// <param name="typeMapping">The type mapping.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the given <see cref="ValueComparer" /> can be configured.
    /// </returns>
    bool CanSetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, or <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasValueComparer(ValueComparer? comparer, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given <see cref="ValueComparer" />
    ///     can be configured from the current configuration source.
    /// </summary>
    /// <param name="comparer">The comparer, or <see langword="null" /> to remove any previously set comparer.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the given <see cref="ValueComparer" /> can be configured.
    /// </returns>
    bool CanSetValueComparer(ValueComparer? comparer, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <param name="comparerType">
    ///     A type that derives from <see cref="ValueComparer" />, or <see langword="null" /> to remove any previously set comparer.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, or <see langword="null" /> otherwise.
    /// </returns>
    IConventionElementTypeBuilder? HasValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given <see cref="ValueComparer" />
    ///     can be configured from the current configuration source.
    /// </summary>
    /// <param name="comparerType">
    ///     A type that derives from <see cref="ValueComparer" />, or <see langword="null" /> to remove any previously set comparer.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     <see langword="true" /> if the given <see cref="ValueComparer" /> can be configured.
    /// </returns>
    bool CanSetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation = false);
}
