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

        public virtual int SqliteErrorCode { get; }
    }
}
