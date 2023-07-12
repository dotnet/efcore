// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure result column.
/// </summary>
public interface IConventionStoredProcedureResultColumn : IReadOnlyStoredProcedureResultColumn, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this result column belongs.
    /// </summary>
    new IConventionStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this result column.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the stored procedure result column has been removed from the model.</exception>
    new IConventionStoredProcedureResultColumnBuilder Builder { get; }

    /// <summary>
    ///     Sets the result column name.
    /// </summary>
    /// <param name="name">The result column name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    string? SetName(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyStoredProcedureResultColumn.Name" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyStoredProcedureResultColumn.Name" />.</returns>
    ConfigurationSource? GetNameConfigurationSource();
}
