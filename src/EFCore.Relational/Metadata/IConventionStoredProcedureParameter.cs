// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure parameter.
/// </summary>
public interface IConventionStoredProcedureParameter : IReadOnlyStoredProcedureParameter, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this parameter belongs.
    /// </summary>
    new IConventionStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this stored procedure parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the stored procedure parameter has been removed from the model.</exception>
    new IConventionStoredProcedureParameterBuilder Builder { get; }

    /// <summary>
    ///     Sets the parameter name.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    string SetName(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyStoredProcedureParameter.Name" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedureParameter.Name" />.</returns>
    ConfigurationSource? GetNameConfigurationSource();

    /// <summary>
    ///     Sets the direction of the parameter.
    /// </summary>
    /// <param name="direction">The direction of the parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    ParameterDirection SetDirection(ParameterDirection direction, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyStoredProcedureParameter.Direction" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedureParameter.Direction" />.</returns>
    ConfigurationSource? GetDirectionConfigurationSource();
}
