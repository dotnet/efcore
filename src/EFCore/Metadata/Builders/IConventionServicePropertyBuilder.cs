// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

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
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionServicePropertyBuilder : IConventionPropertyBaseBuilder<IConventionServicePropertyBuilder>
{
    /// <summary>
    ///     Gets the service property being configured.
    /// </summary>
    new IConventionServiceProperty Metadata { get; }

    /// <summary>
    ///     Sets the <see cref="ServiceParameterBinding" /> for this property.
    /// </summary>
    /// <param name="parameterBinding">The parameter binding.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
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
