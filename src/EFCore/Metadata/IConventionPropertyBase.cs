// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Base type for navigation and scalar properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IPropertyBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionPropertyBase : IPropertyBase, IConventionAnnotatable
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
        ///         <see cref="ConventionPropertyBaseExtensions.SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldInfo"> The <see cref="FieldInfo" /> for the underlying CLR field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="FieldInfo" />. </returns>
        FieldInfo SetFieldInfo([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="FieldInfo" /> for the underlying CLR field that this property should use.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="ConventionPropertyBaseExtensions.SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldInfo"> The <see cref="FieldInfo" /> for the underlying CLR field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use SetFieldInfo")]
        void SetField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false)
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
        ///         <see cref="ConventionPropertyBaseExtensions.SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The name of the field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="FieldInfo" />. </returns>
        FieldInfo SetField([CanBeNull] string fieldName, bool fromDataAnnotation = false)
            => this.AsPropertyBase()
                .SetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="IPropertyBase.FieldInfo" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IPropertyBase.FieldInfo" />. </returns>
        ConfigurationSource? GetFieldInfoConfigurationSource();
    }
}
