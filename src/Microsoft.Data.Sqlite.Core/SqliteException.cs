// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a SQLite error.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
    public class SqliteException : DbException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteException" /> class.
        /// </summary>
        /// <param name="message">The message to display for the exception. Can be null.</param>
        /// <param name="errorCode">The SQLite error code.</param>
        public SqliteException(string? message, int errorCode)
            : this(message, errorCode, errorCode)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteException" /> class.
        /// </summary>
        /// <param name="message">The message to display for the exception. Can be null.</param>
        /// <param name="errorCode">The SQLite error code.</param>
        /// <param name="extendedErrorCode">The extended SQLite error code.</param>
        public SqliteException(string? message, int errorCode, int extendedErrorCode)
            : base(message)
        {
            SqliteErrorCode = errorCode;
            SqliteExtendedErrorCode = extendedErrorCode;
        }

        /// <summary>
        ///     Gets the SQLite error code.
        /// </summary>
        /// <value>The SQLite error code.</value>
        /// <seealso href="https://www.sqlite.org/rescode.html">SQLite Result Codes</seealso>
        public virtual int SqliteErrorCode { get; }

        /// <summary>
        ///     Gets the extended SQLite error code.
        /// </summary>
        /// <value>The SQLite error code.</value>
        /// <seealso href="https://www.sqlite.org/rescode.html#extrc">SQLite Result Codes</seealso>
        public virtual int SqliteExtendedErrorCode { get; }

        /// <summary>
        ///     Throws an exception with a specific SQLite error code value.
        /// </summary>
        /// <param name="rc">The SQLite error code corresponding to the desired exception.</param>
        /// <param name="db">A handle to database connection.</param>
        /// <remarks>
        ///     No exception is thrown for non-error result codes.
        /// </remarks>
        public static void ThrowExceptionForRC(int rc, sqlite3? db)
        {
            if (rc is SQLITE_OK or SQLITE_ROW or SQLITE_DONE)
            {
                return;
            }

            string message;
            int extendedErrorCode;
            if (db == null
                || db.IsInvalid
                || rc != sqlite3_errcode(db))
            {
                message = sqlite3_errstr(rc).utf8_to_string() + " " + Resources.DefaultNativeError;
                extendedErrorCode = rc;
            }
            else
            {
                message = sqlite3_errmsg(db).utf8_to_string();
                extendedErrorCode = sqlite3_extended_errcode(db);
            }

            throw new SqliteException(Resources.SqliteNativeError(rc, message), rc, extendedErrorCode);
        }
    }
}
