// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a SQLite error.
    /// </summary>
    public class SqliteException : DbException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteException" /> class.
        /// </summary>
        /// <param name="message">The message to display for the exception. Can be null.</param>
        /// <param name="errorCode">The SQLite error code.</param>
        public SqliteException(string message, int errorCode)
            : base(message)
            => SqliteErrorCode = errorCode;

        /// <summary>
        /// Gets the SQLite error code.
        /// </summary>
        /// <value>The SQLite error code.</value>
        /// <seealso href="http://sqlite.org/rescode.html">SQLite Result Codes</seealso>
        public virtual int SqliteErrorCode { get; }

        /// <summary>
        /// Throws an exception with a specific SQLite error code value.
        /// </summary>
        /// <param name="rc">The SQLite error code corresponding to the desired exception.</param>
        /// <param name="db">A handle to database connection.</param>
        /// <remarks>
        /// No exception is thrown forn non-error result codes.
        /// </remarks>
        public static void ThrowExceptionForRC(int rc, sqlite3 db)
        {
            if (rc == raw.SQLITE_OK
                || rc == raw.SQLITE_ROW
                || rc == raw.SQLITE_DONE)
            {
                return;
            }

            var message = db == null || db.ptr == IntPtr.Zero
                ? raw.sqlite3_errstr(rc) + " " + Resources.DefaultNativeError
                : raw.sqlite3_errmsg(db);

            throw new SqliteException(Resources.SqliteNativeError(rc, message), rc);
        }
    }
}
