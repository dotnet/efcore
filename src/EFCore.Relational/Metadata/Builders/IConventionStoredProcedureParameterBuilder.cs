// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IConventionStoredProcedureParameter" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionStoredProcedureParameterBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The stored procedure parameter metadata that is being built.
    /// </summary>
    new IConventionStoredProcedureParameter Metadata { get; }

    /// <summary>
    ///     Configures the parameter name.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureParameterBuilder? HasName(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given parameter name can be set.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given parameter name can be set.</returns>
    bool CanSetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the direction of the stored procedure parameter.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionStoredProcedureParameterBuilder? HasDirection(ParameterDirection direction, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given direction can be configured on the corresponding stored procedure parameter.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given direction can be configured.</returns>
    bool CanSetDirection(ParameterDirection direction, bool fromDataAnnotation = false);    
}
