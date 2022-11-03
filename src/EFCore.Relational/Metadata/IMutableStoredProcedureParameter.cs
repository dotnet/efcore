// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure parameter.
/// </summary>
public interface IMutableStoredProcedureParameter : IReadOnlyStoredProcedureParameter, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this parameter belongs.
    /// </summary>
    new IMutableStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets or sets the parameter name.
    /// </summary>
    new string Name { get; set; }

    /// <summary>
    ///     Gets or sets the direction of the parameter.
    /// </summary>
    new ParameterDirection Direction { get; set; }
}
