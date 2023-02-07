// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a stored procedure result column.
/// </summary>
public interface IMutableStoredProcedureResultColumn : IReadOnlyStoredProcedureResultColumn, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the stored procedure to which this result column belongs.
    /// </summary>
    new IMutableStoredProcedure StoredProcedure { get; }

    /// <summary>
    ///     Gets or sets the result column name.
    /// </summary>
    new string Name { get; set; }
}
