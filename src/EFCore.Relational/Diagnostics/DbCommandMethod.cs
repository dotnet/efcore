// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
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
}
