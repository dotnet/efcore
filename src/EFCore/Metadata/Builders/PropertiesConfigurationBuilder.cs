// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
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
public class PropertiesConfigurationBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public PropertiesConfigurationBuilder(PropertyConfiguration property)
    {
        Check.NotNull(property, nameof(property));

        Configuration = property;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual PropertyConfiguration Configuration { get; }

    /// <summary>
    ///     Adds or updates an annotation on the property.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveAnnotation(string annotation, object value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Configuration[annotation] = value;

        return this;
    }

    /// <summary>
    ///     Configures the maximum length of data that can be stored in this property.
    ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
    /// </summary>
    /// <param name="maxLength">The maximum length of data allowed in the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveMaxLength(int maxLength)
    {
        Configuration.SetMaxLength(maxLength);

        return this;
    }

    /// <summary>
    ///     Configures the value that will be used to determine if the property has been set or not. If the property is set to the
    ///     sentinel value, then it is considered not set. By default, the sentinel value is the CLR default value for the type of
    ///     the property.
    /// </summary>
    /// <param name="sentinel">The sentinel value.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveSentinel(object? sentinel)
    {
        Configuration.SetSentinel(sentinel);

        return this;
    }

    /// <summary>
    ///     Configures the precision and scale of the property.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    /// <param name="scale">The scale of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HavePrecision(int precision, int scale)
    {
        Configuration.SetPrecision(precision);
        Configuration.SetScale(scale);

        return this;
    }

    /// <summary>
    ///     Configures the precision of the property.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HavePrecision(int precision)
    {
        Configuration.SetPrecision(precision);

        return this;
    }

    /// <summary>
    ///     Configures whether the property as capable of persisting unicode characters.
    ///     Can only be set on <see cref="string" /> properties.
    /// </summary>
    /// <param name="unicode">A value indicating whether the property can contain unicode characters.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder AreUnicode(bool unicode = true)
    {
        Configuration.SetIsUnicode(unicode);

        return this;
    }

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion<TConversion>()
        => HaveConversion(typeof(TConversion));

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion(Type conversionType)
    {
        Check.NotNull(conversionType, nameof(conversionType));

        if (typeof(ValueConverter).IsAssignableFrom(conversionType))
        {
            Configuration.SetValueConverter(conversionType);
        }
        else
        {
            Configuration.SetProviderClrType(conversionType);
        }

        return this;
    }

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <typeparam name="TComparer">A type that inherits from <see cref="ValueComparer" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion<TConversion, TComparer>()
        where TComparer : ValueComparer
        => HaveConversion(typeof(TConversion), typeof(TComparer));

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <typeparam name="TComparer">A type that inherits from <see cref="ValueComparer" />.</typeparam>
    /// <typeparam name="TProviderComparer">A type that inherits from <see cref="ValueComparer" /> to use for the provider values.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion<TConversion, TComparer, TProviderComparer>()
        where TComparer : ValueComparer
        => HaveConversion(typeof(TConversion), typeof(TComparer), typeof(TProviderComparer));

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <param name="comparerType">A type that inherits from <see cref="ValueComparer" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion(Type conversionType, Type? comparerType)
        => HaveConversion(conversionType, comparerType, null);

    /// <summary>
    ///     Configures the property so that the property value is converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <param name="comparerType">A type that inherits from <see cref="ValueComparer" />.</param>
    /// <param name="providerComparerType">A type that inherits from <see cref="ValueComparer" /> to use for the provider values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual PropertiesConfigurationBuilder HaveConversion(Type conversionType, Type? comparerType, Type? providerComparerType)
    {
        Check.NotNull(conversionType, nameof(conversionType));

        if (typeof(ValueConverter).IsAssignableFrom(conversionType))
        {
            Configuration.SetValueConverter(conversionType);
        }
        else
        {
            Configuration.SetProviderClrType(conversionType);
        }

        Configuration.SetValueComparer(comparerType);

        Configuration.SetProviderValueComparer(providerComparerType);

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
