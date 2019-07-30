// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;

        public SqlServerQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual QuerySqlGenerator Create()
            => new SqlServerQuerySqlGenerator(_dependencies);
    }
}
