// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.Data.SQLite.Utilities
{
    internal static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            var value = connection.ExecuteScalar(commandText);
            if (!(value is T))
                return default(T);

            return (T)value;
        }

        private static object ExecuteScalar(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteScalar();
            }
        }
    }
}
