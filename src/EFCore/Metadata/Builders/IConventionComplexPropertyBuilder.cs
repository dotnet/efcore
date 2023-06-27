// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionComplexProperty" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionComplexPropertyBuilder : IConventionPropertyBaseBuilder<IConventionComplexPropertyBuilder>
{
    /// <summary>
    ///     Gets the property being configured.
    /// </summary>
    new IConventionComplexProperty Metadata { get; }

    /// <summary>
    ///     Configures whether this property must have a value assigned or <see langword="null" /> is a valid value.
    ///     A property can only be configured as non-required if it is based on a CLR type that can be
    ///     assigned <see langword="null" />.
    /// </summary>
    /// <param name="required">
    ///     A value indicating whether the property is required.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the requiredness was configured,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionComplexPropertyBuilder? IsRequired(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this property requiredness can be configured
    ///     from the current configuration source.
    /// </summary>
    /// <param name="required">
    ///     A value indicating whether the property is required.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property requiredness can be configured.</returns>
    bool CanSetIsRequired(bool? required, bool fromDataAnnotation = false);
}
