// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.Sqlite
{
    public class SqliteException : DbException
    {
        public SqliteException(string message, int errorCode)
            : base(message)
        {
            SqliteErrorCode = errorCode;
        }

        /// <summary>
        /// The error code produced by SQLite. The value's meaning depends on the context in which the exception is thrown. 
        /// <see href="https://www.sqlite.org/rescode.html">See SQLite.org for a list of error codes.</see>
        /// </summary>
        public virtual int SqliteErrorCode { get; }
    }
}
