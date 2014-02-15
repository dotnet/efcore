// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.SQLite
{
    public class SQLiteException : DbException
    {
        public SQLiteException(string message, int errorCode)
            : base(message, errorCode)
        {
        }
    }
}
