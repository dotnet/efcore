// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Enum used by <see cref="CommandEventData" />, an subclasses to indicate the
///     method on <see cref="DbCommand" /> being used to execute the command.
/// </summary>
public enum DbCommandMethod
{
    /// <summary>
    ///     The <see cref="DbCommand.ExecuteNonQuery" /> or
    ///     <see cref="DbCommand.ExecuteNonQueryAsync()" /> method.
    /// </summary>
    ExecuteNonQuery,

    /// <summary>
    ///     The <see cref="DbCommand.ExecuteScalar" /> or
    ///     <see cref="DbCommand.ExecuteScalarAsync()" /> method.
    /// </summary>
    ExecuteScalar,

    /// <summary>
    ///     The <see cref="DbCommand.ExecuteReader()" /> or
    ///     <see cref="DbCommand.ExecuteReaderAsync()" /> method.
    /// </summary>
    ExecuteReader
}
