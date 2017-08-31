// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    using SQLitePCL;
    /// <summary>
    /// Represents the event that changed the table data.
    /// </summary>
    public enum UpdateEventType
    {
        /// <summary>
        /// Row was updated.
        /// </summary>
        Update = raw.SQLITE_UPDATE,

        /// <summary>
        /// Row was deleted.
        /// </summary>
        Delete = raw.SQLITE_DELETE,

        /// <summary>
        /// Row was inserted.
        /// </summary>
        Insert = raw.SQLITE_INSERT
    }
}