// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents the connection modes that can be used when opening a connection.
    /// </summary>
    public enum SqliteOpenMode
    {
        /// <summary>
        ///     Opens the database for reading and writing, and creates it if it doesn't exist.
        /// </summary>
        ReadWriteCreate,

        /// <summary>
        ///     Opens the database for reading and writing.
        /// </summary>
        ReadWrite,

        /// <summary>
        ///     Opens the database in read-only mode.
        /// </summary>
        ReadOnly,

        /// <summary>
        ///     Opens an in-memory database.
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/in-memory-databases">In-Memory Databases</seealso>
        Memory
    }
}
