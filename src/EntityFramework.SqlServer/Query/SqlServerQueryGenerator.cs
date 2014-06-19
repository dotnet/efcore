// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Query.Sql;

namespace Microsoft.Data.Entity.SqlServer.Query
{
    public class SqlServerQueryGenerator : DefaultSqlQueryGenerator
    {
        protected override string DelimitIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
