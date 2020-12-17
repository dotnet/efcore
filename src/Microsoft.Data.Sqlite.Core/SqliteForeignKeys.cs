// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents the foreign key constraint enforcement modes of SQLite.
    /// </summary>
    public enum SqliteForeignKeys
    {
        /// <summary>
        ///     The database default.
        /// </summary>
        Default = -1,

        /// <summary>
        ///     Don't enforce foreign key constraints.
        /// </summary>
        Off,

        /// <summary>
        ///     Enforce foreign key constraints.
        /// </summary>
        On
    }
}
