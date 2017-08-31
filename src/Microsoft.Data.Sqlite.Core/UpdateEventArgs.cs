// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    using System;

    /// <summary>
    /// Provides data for the Update event of SqliteConnection.
    /// </summary>
    public class UpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateEventArgs"/> class.
        /// </summary>
        /// <param name="eventType">The event that changed the data.</param>
        /// <param name="database">The database name.</param>
        /// <param name="table">The table name.</param>
        /// <param name="rowId">The rowid of the affected row.</param>
        public UpdateEventArgs(UpdateEventType eventType, string database, string table, long rowId)
        {
            Event = eventType;
            Database = database;
            Table = table;
            RowId = rowId;
        }

        /// <summary>
        /// Gets the event that changed the data.
        /// </summary>
        /// <value>
        /// The event that changed the data.
        /// </value>
        public UpdateEventType Event { get; }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        /// <value>
        /// The database name.
        /// </value>
        public string Database { get; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        public string Table { get; }

        /// <summary>
        /// Gets the rowid of the affected row.
        /// </summary>
        /// <value>
        /// The rowid of the affected row.
        /// </value>
        public long RowId { get; }
    }
}