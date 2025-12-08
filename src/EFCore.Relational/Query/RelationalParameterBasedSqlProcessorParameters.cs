// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Parameters for <see cref="RelationalParameterBasedSqlProcessor" />.
/// </summary>
public sealed record RelationalParameterBasedSqlProcessorParameters
{
    /// <summary>
    ///     A value indicating if relational nulls should be used.
    /// </summary>
    public bool UseRelationalNulls { get; init; }

    /// <summary>
    ///     A collection of parameter names to constantize.
    /// </summary>
    public IReadOnlySet<string> ParametersToConstantize { get; init; }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalParameterBasedSqlProcessorParameters" />.
    /// </summary>
    /// <param name="useRelationalNulls">A value indicating if relational nulls should be used.</param>
    /// <param name="parametersToConstantize">A collection of parameter names to constantize.</param>
    [EntityFrameworkInternal]
    public RelationalParameterBasedSqlProcessorParameters(bool useRelationalNulls, IReadOnlySet<string> parametersToConstantize)
    {
        UseRelationalNulls = useRelationalNulls;
        ParametersToConstantize = parametersToConstantize;
    }
}
