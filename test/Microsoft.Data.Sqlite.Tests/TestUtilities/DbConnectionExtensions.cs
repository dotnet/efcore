// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.Sqlite.TestUtilities
{
    internal static class DbConnectionExtensions
    {
        public static DbDataReader ExecuteReader(this DbConnection connection, string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;

            return command.ExecuteReader();
        }
    }
}
