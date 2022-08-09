// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting property defaults before conventions run.
/// </summary>
/// <remarks>
///     Instances of this class are returned from methods when using the <see cref="ModelConfigurationBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class PropertiesConfigurationBuilder<TProperty> : PropertiesConfigurationBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public PropertiesConfigurationBuilder(PropertyConfiguration property)
        : base(property)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the property. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveAnnotation(string annotation, object value)
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveAnnotation(annotation, value);

    /// <summary>
    ///     Configures the maximum length of data that can be stored in this property.
    ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
    /// </summary>
    /// <param name="maxLength">The maximum length of data allowed in the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveMaxLength(int maxLength)
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveMaxLength(maxLength);

    /// <summary>
    ///     Configures the precision and scale of the property.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    /// <param name="scale">The scale of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HavePrecision(int precision, int scale)
        => (PropertiesConfigurationBuilder<TProperty>)base.HavePrecision(precision, scale);

    /// <summary>
    ///     Configures the precision of the property.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HavePrecision(int precision)
        => (PropertiesConfigurationBuilder<TProperty>)base.HavePrecision(precision);

    /// <summary>
    ///     Configures the property as capable of persisting unicode characters.
    ///     Can only be set on <see cref="string" /> properties.
    /// </summary>
    /// <param name="unicode">A value indicating whether the property can contain unicode characters.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> AreUnicode(bool unicode = true)
        => (PropertiesConfigurationBuilder<TProperty>)base.AreUnicode(unicode);

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion<TConversion>()
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion<TConversion>();

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion(Type conversionType)
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion(conversionType);

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <typeparam name="TComparer">A type that inherits from <see cref="ValueComparer" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion<TConversion, TComparer>()
        where TComparer : ValueComparer
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion<TConversion, TComparer>();

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <param name="comparerType">A type that inherits from <see cref="ValueComparer" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion(Type conversionType, Type? comparerType)
        => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion(conversionType, comparerType);
}
