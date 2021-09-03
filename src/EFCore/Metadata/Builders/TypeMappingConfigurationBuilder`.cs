// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for setting property defaults before conventions run.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelConfigurationBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public class TypeMappingConfigurationBuilder<TProperty> : TypeMappingConfigurationBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public TypeMappingConfigurationBuilder(PropertyConfiguration scalar)
            : base(scalar)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the property. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasAnnotation(string annotation, object value)
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasMaxLength(int maxLength)
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasMaxLength(maxLength);

        /// <summary>
        ///     Configures the precision and scale of the property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasPrecision(int precision, int scale)
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasPrecision(precision, scale);

        /// <summary>
        ///     <para>
        ///         Configures the precision of the property.
        ///     </para>
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasPrecision(int precision)
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasPrecision(precision);

        /// <summary>
        ///     Configures the property as capable of persisting unicode characters.
        ///     Can only be set on <see cref="string" /> properties.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> IsUnicode(bool unicode = true)
            => (TypeMappingConfigurationBuilder<TProperty>)base.IsUnicode(unicode);

        /// <summary>
        ///     Configures the property so that the property value is converted before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TConversion"> The type to convert to and from or a type that derives from <see cref="ValueConverter"/>. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasConversion<TConversion>()
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasConversion<TConversion>();

        /// <summary>
        ///     Configures the property so that the property value is converted before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="conversionType"> The type to convert to and from or a type that derives from <see cref="ValueConverter"/>. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TypeMappingConfigurationBuilder<TProperty> HasConversion(Type conversionType)
            => (TypeMappingConfigurationBuilder<TProperty>)base.HasConversion(conversionType);
    }
}
