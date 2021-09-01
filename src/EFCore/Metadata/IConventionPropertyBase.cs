// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Base type for navigation and scalar properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IReadOnlyPropertyBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionPropertyBase : IReadOnlyPropertyBase, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IConventionTypeBase DeclaringType { get; }

        /// <summary>
        ///     Returns the configuration source for this property.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="FieldInfo" /> for the underlying CLR field that this property should use.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldInfo"> The <see cref="FieldInfo" /> for the underlying CLR field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="FieldInfo" />. </returns>
        FieldInfo? SetFieldInfo(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="FieldInfo" /> for the underlying CLR field that this property should use.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldInfo"> The <see cref="FieldInfo" /> for the underlying CLR field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use SetFieldInfo")]
        void SetField(FieldInfo? fieldInfo, bool fromDataAnnotation = false)
            => SetFieldInfo(fieldInfo, fromDataAnnotation);

        /// <summary>
        ///     <para>
        ///         Sets the underlying CLR field that this property should use.
        ///         This may be <see langword="null" /> for shadow properties or if the backing field for the property is not known.
        ///     </para>
        ///     <para>
        ///         Backing fields are normally found by convention as described
        ///         here: http://go.microsoft.com/fwlink/?LinkId=723277.
        ///         This method is useful for setting backing fields explicitly in cases where the
        ///         correct field is not found by convention.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The name of the field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="FieldInfo" />. </returns>
        FieldInfo? SetField(string? fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyPropertyBase.FieldInfo" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IReadOnlyPropertyBase.FieldInfo" />. </returns>
        ConfigurationSource? GetFieldInfoConfigurationSource();

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        PropertyAccessMode? SetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyPropertyBase.GetPropertyAccessMode" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IReadOnlyPropertyBase.GetPropertyAccessMode" />. </returns>
        ConfigurationSource? GetPropertyAccessModeConfigurationSource();
    }
}
