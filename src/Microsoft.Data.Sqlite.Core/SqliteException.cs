// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a SQLite error.
    /// </summary>
    public class SqliteException : DbException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteException" /> class.
        /// </summary>
        /// <param name="message">The message to display for the exception. Can be null.</param>
        /// <param name="errorCode">The SQLite error code.</param>
        public SqliteException(string message, int errorCode)
            : this(message, errorCode, errorCode)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteException" /> class.
        /// </summary>
        /// <param name="message">The message to display for the exception. Can be null.</param>
        /// <param name="errorCode">The SQLite error code.</param>
        /// /// <param name="extendedErrorCode">The extended SQLite error code.</param>
        public SqliteException(string message, int errorCode, int extendedErrorCode)
            : base(message)
        {
            SqliteErrorCode = errorCode;
            SqliteExtendedErrorCode = extendedErrorCode;
        }

        /// <summary>
        ///     Gets the SQLite error code.
        /// </summary>
        /// <value>The SQLite error code.</value>
        /// <seealso href="http://sqlite.org/rescode.html">SQLite Result Codes</seealso>
        public virtual int SqliteErrorCode { get; }

        /// <summary>
        ///     Gets the extended SQLite error code.
        /// </summary>
        /// <value>The SQLite error code.</value>
        /// <seealso href="https://sqlite.org/rescode.html#extrc">SQLite Result Codes</seealso>
        public virtual int SqliteExtendedErrorCode { get; }

        /// <summary>
        ///     Throws an exception with a specific SQLite error code value.
        /// </summary>
        /// <param name="rc">The SQLite error code corresponding to the desired exception.</param>
        /// <param name="db">A handle to database connection.</param>
        /// <remarks>
        ///     No exception is thrown for non-error result codes.
        /// </remarks>
        public static void ThrowExceptionForRC(int rc, sqlite3 db)
        {
            if (rc == raw.SQLITE_OK
                || rc == raw.SQLITE_ROW
                || rc == raw.SQLITE_DONE)
            {
                return;
            }

            string message;
            int extendedErrorCode;
            if (db == null || db.ptr == IntPtr.Zero || rc != raw.sqlite3_errcode(db))
            {
                message = raw.sqlite3_errstr(rc) + " " + Resources.DefaultNativeError;
                extendedErrorCode = rc;
            }
            else
            {
                message = raw.sqlite3_errmsg(db);
                extendedErrorCode = raw.sqlite3_extended_errcode(db);
            }

            throw new SqliteException(Resources.SqliteNativeError(rc, message), rc, extendedErrorCode);
        }
    }
}
