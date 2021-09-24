// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionServiceProperty" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionServicePropertyBuilder : IConventionPropertyBaseBuilder
    {
        /// <summary>
        ///     Gets the service property being configured.
        /// </summary>
        new IConventionServiceProperty Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this property.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        new IConventionServicePropertyBuilder? HasField(string? fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this property.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        new IConventionServicePropertyBuilder? HasField(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
        /// </summary>
        /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        new IConventionServicePropertyBuilder? UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        /// <param name="parameterBinding">The parameter binding.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        ///</returns>
        IConventionServicePropertyBuilder? HasParameterBinding(
            ServiceParameterBinding? parameterBinding,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="ServiceParameterBinding" /> can be set for this property.
        ///     from the current configuration source.
        /// </summary>
        /// <param name="parameterBinding">The parameter binding.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the <see cref="ServiceParameterBinding" /> can be set for this property.</returns>
        bool CanSetParameterBinding(ServiceParameterBinding? parameterBinding, bool fromDataAnnotation = false);
    }
}
