// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionPropertyBase" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionPropertyBaseBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the property-like object being configured.
        /// </summary>
        new IConventionPropertyBase Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this property-like object.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBaseBuilder HasField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this property-like object.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBaseBuilder HasField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the backing field can be set for this property-like object. </returns>
        bool CanSetField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the backing field can be set for this property-like object. </returns>
        bool CanSetField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property-like object.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this property-like object. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBaseBuilder UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="PropertyAccessMode" /> can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this property-like object. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the <see cref="PropertyAccessMode" /> can be set for this property-like object. </returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
    }
}
