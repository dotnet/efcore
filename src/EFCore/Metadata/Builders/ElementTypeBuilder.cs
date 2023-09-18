// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring the <see cref="IMutableElementType" /> of a primitive collection.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class ElementTypeBuilder : IInfrastructure<IConventionElementTypeBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ElementTypeBuilder(IMutableElementType elementType)
    {
        Check.NotNull(elementType, nameof(elementType));

        Builder = ((ElementType)elementType).Builder;
    }

    /// <summary>
    ///     The internal builder being used to configure the element type.
    /// </summary>
    IConventionElementTypeBuilder IInfrastructure<IConventionElementTypeBuilder>.Instance
        => Builder;

    private InternalElementTypeBuilder Builder { get; }

    /// <summary>
    ///     The element type being configured.
    /// </summary>
    public virtual IMutableElementType Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the element type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures whether elements of the collection must have a value or can be <see langword="null" />.
    ///     An element can only be configured as non-required if it is based on a CLR type that can be
    ///     assigned <see langword="null" />.
    /// </summary>
    /// <param name="required">A value indicating whether elements of the collection must not be <see langword="null" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder IsRequired(bool required = true)
    {
        Builder.IsRequired(required, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures the maximum length of data that can be stored in elements of the collection.
    /// </summary>
    /// <param name="maxLength">
    ///     The maximum length of data allowed in elements of the collection. A value of <c>-1</c> indicates that elements of the
    ///     collection have no maximum length.
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasMaxLength(int maxLength)
    {
        Builder.HasMaxLength(maxLength, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures the precision and scale of elements of the collection.
    /// </summary>
    /// <param name="precision">The precision of elements of the collection.</param>
    /// <param name="scale">The scale of elements of the collection.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasPrecision(int precision, int scale)
    {
        Builder.HasPrecision(precision, ConfigurationSource.Explicit);
        Builder.HasScale(scale, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures the precision of elements of the collection.
    /// </summary>
    /// <param name="precision">The precision of elements of the collection.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasPrecision(int precision)
    {
        Builder.HasPrecision(precision, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures whether elements of the collection are capable of persisting unicode characters.
    /// </summary>
    /// <param name="unicode">A value indicating whether elements of the collection can contain unicode characters.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder IsUnicode(bool unicode = true)
    {
        Builder.IsUnicode(unicode, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures elements of the collection so their values are converted before writing to the database and converted back
    ///     when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TConversion>()
        => HasConversion(typeof(TConversion));

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before writing to the database and converted back
    ///     when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? conversionType)
    {
        if (typeof(ValueConverter).IsAssignableFrom(conversionType))
        {
            Builder.HasConverter(conversionType, ConfigurationSource.Explicit);
        }
        else
        {
            Builder.HasConversion(conversionType, ConfigurationSource.Explicit);
        }

        return this;
    }

    /// <summary>
    ///     Configures elements of the collection so that their values are converted to and from the database
    ///     using the given <see cref="ValueConverter" />.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion(ValueConverter? converter)
        => HasConversion(converter, null);

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="valueComparer">The comparer to use for values before conversion.</param>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TConversion>(
        ValueComparer? valueComparer)
        => HasConversion(typeof(TConversion), valueComparer);

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <param name="valueComparer">The comparer to use for values before conversion.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type conversionType,
        ValueComparer? valueComparer)
    {
        Check.NotNull(conversionType, nameof(conversionType));

        if (typeof(ValueConverter).IsAssignableFrom(conversionType))
        {
            Builder.HasConverter(conversionType, ConfigurationSource.Explicit);
        }
        else
        {
            Builder.HasConversion(conversionType, ConfigurationSource.Explicit);
        }

        Builder.HasValueComparer(valueComparer, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before
    ///     using the given <see cref="ValueConverter" />.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <param name="valueComparer">The comparer to use for values before conversion.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion(ValueConverter? converter, ValueComparer? valueComparer)
    {
        Builder.HasConversion(converter, ConfigurationSource.Explicit);
        Builder.HasValueComparer(valueComparer, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <typeparam name="TConversion">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</typeparam>
    /// <typeparam name="TComparer">A type that inherits from <see cref="ValueComparer" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TConversion,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TComparer>()
        where TComparer : ValueComparer
        => HasConversion(typeof(TConversion), typeof(TComparer));

    /// <summary>
    ///     Configures elements of the collection so that their values are converted before
    ///     writing to the database and converted back when reading from the database.
    /// </summary>
    /// <param name="conversionType">The type to convert to and from or a type that inherits from <see cref="ValueConverter" />.</param>
    /// <param name="comparerType">A type that inherits from <see cref="ValueComparer" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ElementTypeBuilder HasConversion(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type conversionType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType)
    {
        Check.NotNull(conversionType, nameof(conversionType));

        if (typeof(ValueConverter).IsAssignableFrom(conversionType))
        {
            Builder.HasConverter(conversionType, ConfigurationSource.Explicit);
        }
        else
        {
            Builder.HasConversion(conversionType, ConfigurationSource.Explicit);
        }

        Builder.HasValueComparer(comparerType, ConfigurationSource.Explicit);

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
