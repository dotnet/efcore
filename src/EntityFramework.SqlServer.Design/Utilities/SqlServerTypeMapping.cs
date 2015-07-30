// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.SqlServer.Design.Utilities
{
    public static class SqlServerTypeMapping
    {
        public static readonly Dictionary<string, Type> _sqlTypeToClrTypeMap
            = new Dictionary<string, Type>
            {
                // exact numerics
                { "bigint", typeof(long) },
                { "bit", typeof(bool) },
                { "decimal", typeof(decimal) },
                { "int", typeof(int) },
                { "money", typeof(decimal) },
                { "numeric", typeof(decimal) },
                { "smallint", typeof(short) },
                { "smallmoney", typeof(decimal) },
                { "tinyint", typeof(byte) },

                // approximate numerics
                { "float", typeof(double) }, // This is correct. SQL Server 'float' type maps to C# double
                { "real", typeof(float) },

                // date and time
                { "date", typeof(DateTime) },
                { "datetime", typeof(DateTime) },
                { "datetime2", typeof(DateTime) },
                { "datetimeoffset", typeof(DateTimeOffset) },
                { "smalldatetime", typeof(DateTime) },
                { "time", typeof(DateTime) },

                // character strings
                { "char", typeof(string) },
                { "text", typeof(string) },
                { "varchar", typeof(string) },

                // unicode character strings
                { "nchar", typeof(string) },
                { "ntext", typeof(string) },
                { "nvarchar", typeof(string) },

                // binary
                { "binary", typeof(byte[]) },
                { "image", typeof(byte[]) },
                { "varbinary", typeof(byte[]) },

                //TODO other
                //{ "cursor", typeof(TODO) },
                //{ "hierarchyid", typeof(TODO) },
                //{ "sql_variant", typeof(TODO) },
                //{ "table", typeof(TODO) },
                { "timestamp", typeof(byte[]) }, // note: rowversion is a synonym but SQL Server stores the data type as 'timestamp'
                { "uniqueidentifier", typeof(Guid) }
                //{ "xml", typeof(TODO) },

                //TODO spatial
                //{ "geography", typeof(TODO) },
                //{ "geometry", typeof(TODO) },
            };
    }
}
