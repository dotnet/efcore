// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionPropertyBaseBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the property-like object being configured.
        /// </summary>
        new IConventionPropertyBase Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this property-like object.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionPropertyBaseBuilder? HasField(string? fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this property-like object.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionPropertyBaseBuilder? HasField(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the backing field can be set for this property-like object.</returns>
        bool CanSetField(string? fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the backing field can be set for this property-like object.</returns>
        bool CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property-like object.
        /// </summary>
        /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property-like object.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionPropertyBaseBuilder? UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="PropertyAccessMode" /> can be set for this property-like object
        ///     from the current configuration source.
        /// </summary>
        /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property-like object.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the <see cref="PropertyAccessMode" /> can be set for this property-like object.</returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
    }
}
