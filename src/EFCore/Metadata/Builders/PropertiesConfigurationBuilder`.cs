// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveAnnotation(string annotation, object value)
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveAnnotation(annotation, value);

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveMaxLength(int maxLength)
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveMaxLength(maxLength);

        /// <summary>
        ///     Configures the precision and scale of the property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HavePrecision(int precision, int scale)
            => (PropertiesConfigurationBuilder<TProperty>)base.HavePrecision(precision, scale);

        /// <summary>
        ///     <para>
        ///         Configures the precision of the property.
        ///     </para>
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HavePrecision(int precision)
            => (PropertiesConfigurationBuilder<TProperty>)base.HavePrecision(precision);

        /// <summary>
        ///     Configures the property as capable of persisting unicode characters.
        ///     Can only be set on <see cref="string" /> properties.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> AreUnicode(bool unicode = true)
            => (PropertiesConfigurationBuilder<TProperty>)base.AreUnicode(unicode);

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion<TProvider>()
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion<TProvider>();

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion(Type providerClrType)
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion(providerClrType);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <typeparam name="TConverter"> A type that derives from <see cref="ValueConverter"/>. </typeparam>
        /// <typeparam name="TComparer"> A type that derives from <see cref="ValueComparer"/>. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion<TConverter, TComparer>()
            where TConverter : ValueConverter
            where TComparer : ValueComparer
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion<TConverter, TComparer>();

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converterType"> A type that derives from <see cref="ValueConverter"/>. </param>
        /// <param name="comparerType"> A type that derives from <see cref="ValueComparer"/>. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertiesConfigurationBuilder<TProperty> HaveConversion(Type converterType, Type? comparerType)
            => (PropertiesConfigurationBuilder<TProperty>)base.HaveConversion(converterType, comparerType);
    }
}
