// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

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

            Property = property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual PropertyConfiguration Property { get; }

        /// <summary>
        ///     Adds or updates an annotation on the property.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveAnnotation(string annotation, object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));

            Property[annotation] = value;

            return this;
        }

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveMaxLength(int maxLength)
        {
            Property.SetMaxLength(maxLength);

            return this;
        }

        /// <summary>
        ///     Configures the precision and scale of the property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HavePrecision(int precision, int scale)
        {
            Property.SetPrecision(precision);
            Property.SetScale(scale);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the precision of the property.
        ///     </para>
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HavePrecision(int precision)
        {
            Property.SetPrecision(precision);

            return this;
        }

        /// <summary>
        ///     Configures whether the property as capable of persisting unicode characters.
        ///     Can only be set on <see cref="string" /> properties.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder AreUnicode(bool unicode = true)
        {
            Property.SetIsUnicode(unicode);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveConversion<TProvider>()
            => HaveConversion(typeof(TProvider));

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveConversion(Type providerClrType)
        {
            Check.NotNull(providerClrType, nameof(providerClrType));

            Property.SetProviderClrType(providerClrType);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <typeparam name="TConverter"> A type that derives from <see cref="ValueConverter"/>. </typeparam>
        /// <typeparam name="TComparer"> A type that derives from <see cref="ValueComparer"/>. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveConversion<TConverter, TComparer>()
            where TConverter : ValueConverter
            where TComparer : ValueComparer
            => HaveConversion(typeof(TConverter), typeof(TComparer));

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converterType"> A type that derives from <see cref="ValueConverter"/>. </param>
        /// <param name="comparerType"> A type that derives from <see cref="ValueComparer"/>. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertiesConfigurationBuilder HaveConversion(Type converterType, Type? comparerType)
        {
            Check.NotNull(converterType, nameof(converterType));

            Property.SetValueConverter(converterType);
            Property.SetValueComparer(comparerType);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
