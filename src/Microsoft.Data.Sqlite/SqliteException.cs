// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

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
        {
            SqliteErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the SQLite error code.
        /// </summary>
        /// <seealso href="http://sqlite.org/rescode.html">SQLite Result Codes</seealso>
        public virtual int SqliteErrorCode { get; }
    }
}
